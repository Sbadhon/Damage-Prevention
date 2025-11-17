using MediatR;
using SchedulingSvc.Api.Tenancy;
using SchedulingSvc.Application.Common.Behaviors;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Infrastructure.Messaging;
using SchedulingSvc.Infrastructure.WorkOrders;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Options;
using SharedKernel;

namespace SchedulingSvc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //builder.Logging.AddFilter("Microsoft", LogLevel.Information);
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

        // MediatR (use this assembly as the anchor)
        builder.Services.AddMediatR(typeof(Program).Assembly);

        // Tenant plumbing
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));

        // Work order repository (in-memory for now; swap later for EF/Marten)
        builder.Services.AddSingleton<IWorkOrderRepository, InMemoryWorkOrderRepository>();
        builder.Services.AddSingleton<IDateTime, SystemClock>();
        // Bind TicketEvents options so the worker can decide Kafka vs ASB
        builder.Services.Configure<TicketEventsOptions>(
            builder.Configuration.GetSection("TicketEvents"));

        // Background processor that listens to TicketSubmitted events
        builder.Services.AddHostedService<TicketSubmittedEventProcessor>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();

        app.UseCors("AllowFE");

        // later: app.UseAuthentication(); app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
