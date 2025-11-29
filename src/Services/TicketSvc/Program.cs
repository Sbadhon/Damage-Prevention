using MediatR;
using SharedKernel;
using SharedKernel.Options;
using TicketSvc.Application.Tickets.Commands;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Events;
using TicketSvc.Infrastructure.Tickets;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using TicketSvc.Api.Middleware;
using TicketSvc.Api.Tenancy;
using TicketSvc.Application.Common.Behaviors;
using TicketSvc.Application.Common.Tenancy;
using TicketSvc.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;


namespace TicketSvc;

// DTO used for POST /tickets
public record SubmitTicketRequest(string WorkType, string Address, string Description, double Lat, double Lon);

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFE", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // Controllers + Swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // MediatR
        builder.Services.AddMediatR(typeof(SubmitTicketCommand).Assembly);

        // Shared kernel clock
        builder.Services.AddSingleton<IDateTime, SystemClock>();

        // Tenant plumbing
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));

        // EF Core + SQL Server
        var connString = builder.Configuration.GetConnectionString("TicketDatabase")
                  ?? "Host=localhost;Port=5437;Database=TicketDb;Username=postgres;Password=postgres";

        builder.Services.AddDbContext<TicketDbContext>(options =>
            options.UseNpgsql(connString));

        // Use EF repository 
        builder.Services.AddScoped<ITicketRepository, EfTicketRepository>();

        // Outbox repository (for integration events)
        builder.Services.AddSingleton<IOutboxRepository, InMemoryOutboxRepository>();

        builder.Services.AddMassTransit(x =>
        {
            // No consumers in TicketSvc, it's just a publisher
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

        builder.Services.AddHostedService<OutboxDispatcher>();
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Tenant resolution should happen early
        app.UseMiddleware<TenantResolutionMiddleware>();

        app.UseRouting();
        app.UseCors("AllowFE");
        app.MapControllers();

        app.Run();
    }
}
