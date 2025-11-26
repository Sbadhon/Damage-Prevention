using MassTransit;
using MediatR;
using SharedKernel;
using SharedKernel.Options;
using SchedulingSvc.Api.Tenancy;
using SchedulingSvc.Application.Common.Behaviors;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Infrastructure.Messaging;
using SchedulingSvc.Infrastructure.WorkOrders;

namespace SchedulingSvc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // CORS for FE (Vite on http://localhost:5173)
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
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // MediatR
        builder.Services.AddMediatR(typeof(Program).Assembly);
        builder.Services.AddSingleton<IDateTime, SystemClock>();

        // Tenant plumbing
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));

        // In-memory WorkOrder repo
        builder.Services.AddSingleton<IWorkOrderRepository, InMemoryWorkOrderRepository>();

        // MassTransit + RabbitMQ
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

                // This wires up a receive endpoint for TicketSubmittedConsumer
                cfg.ConfigureEndpoints(context);
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.UseCors("AllowFE");
        app.MapControllers();

        app.Run();
    }
}
