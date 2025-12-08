using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Options;
using SchedulingSvc.Api.Tenancy;
using SchedulingSvc.Application.Common.Behaviors;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Infrastructure;
using SchedulingSvc.Infrastructure.Messaging;
using SchedulingSvc.Infrastructure.WorkOrders;
using System.Text.Json.Serialization;
using SchedulingSvc.Api.Middleware;

namespace SchedulingSvc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // CORS for FE 
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFE", policy =>
            {
                policy.WithOrigins(
              "http://localhost:5173",
              "http://localhost:4200"
          )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
            });
        });

        builder.Services.AddControllers()
        .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // MediatR
        builder.Services.AddMediatR(typeof(Program).Assembly);

        // Clock
        builder.Services.AddSingleton<IDateTime, SystemClock>();

        // Tenant pipeline
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));

        // EF Core + SQL Server
        var connString = builder.Configuration.GetConnectionString("SchedulingDatabase")
                          ?? "Server=localhost,1433;Database=SchedulingDb;User Id=sa;Password=SqlStr0ng!Passw0rd;TrustServerCertificate=True;";

        builder.Services.AddDbContext<SchedulingDbContext>(options =>
            options.UseSqlServer(connString));

        // Use EF repository 
        builder.Services.AddScoped<IWorkOrderRepository, EfWorkOrderRepository>();

        // MassTransit + RabbitMQ
        builder.Services.AddMassTransit(x =>
        {
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

        // Ensure DB + table exist
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
            db.Database.EnsureCreated();
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
