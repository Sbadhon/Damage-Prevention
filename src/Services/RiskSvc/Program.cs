using MediatR;
using SharedKernel;
using SharedKernel.Options;
using RiskSvc.Infrastructure.Messaging;
using RiskSvc.Domain.Abstractions;
using RiskSvc.Infrastructure.Risk;
using RiskSvc.Api.Tenancy;
using RiskSvc.Application.Common.Behaviors;
using RiskSvc.Application.Common.Tenancy;

namespace RiskSvc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.WebHost.UseUrls("http://localhost:5279");

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

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddMediatR(typeof(Program).Assembly);

        // Shared clock
        builder.Services.AddSingleton<IDateTime, SystemClock>();

        // Tenancy pipeline
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));

        // In-memory repo
        builder.Services.AddSingleton<IRiskAssessmentRepository, InMemoryRiskAssessmentRepository>();

        // Bind TicketEventsOptions from configuration
        builder.Services.Configure<TicketEventsOptions>(
            builder.Configuration.GetSection("TicketEvents"));

        // Background consumer (Kafka / ASB) with retry
        builder.Services.AddHostedService<TicketRiskAssessmentProcessor>();

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
