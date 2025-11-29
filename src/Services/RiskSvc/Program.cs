using MassTransit;
using MediatR;
using SharedKernel;
using SharedKernel.Options;
using RiskSvc.Api.Tenancy;
using RiskSvc.Application.Common.Behaviors;
using RiskSvc.Application.Common.Tenancy;
using RiskSvc.Domain.Abstractions;
using RiskSvc.Infrastructure.Messaging;
using RiskSvc.Infrastructure.Risk;
using System.Text.Json.Serialization;
using RiskSvc.Api.Middleware;
using Microsoft.EntityFrameworkCore;

namespace RiskSvc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // CORS if you need it (similar to SchedulingSvc)
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFE", policy =>
            {
                policy
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // Controllers + Swagger
        builder.Services.AddControllers()
           .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // MediatR
        builder.Services.AddMediatR(typeof(Program).Assembly);
        builder.Services.AddSingleton<IDateTime, SystemClock>();

        // Tenant plumbing (mirror how you did it in SchedulingSvc)
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));

        // EF Core + SQL Server
        // EF Core + SQL Server with transient retry
        var connString = builder.Configuration.GetConnectionString("RiskDatabase")
                          ?? "Server=localhost,1433;Database=RiskSvcDb;User Id=sa;Password=SqlStr0ng!Passw0rd;TrustServerCertificate=True;";

        builder.Services.AddDbContext<RiskDbContext>(options =>
            options.UseSqlServer(connString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)
            ));



        // EF Core 
        builder.Services.AddScoped<IRiskAssessmentRepository, EfCoreRiskAssessmentRepository>();

        // TicketEvents options (in case you still use them for something else)
        builder.Services.Configure<TicketEventsOptions>(
            builder.Configuration.GetSection("TicketEvents"));

        //  MassTransit + RabbitMQ
        builder.Services.AddMassTransit(x =>
        {
            // Consumer for TicketSubmittedEvent
            x.AddConsumer<TicketSubmittedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        var app = builder.Build();

        // Automatically create/migrate DB on startup
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RiskDbContext>();
            db.Database.Migrate(); // creates the database + applies migrations
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseRouting();
        app.UseCors("AllowFE");
        app.MapControllers();

        app.Run();

    }
}
