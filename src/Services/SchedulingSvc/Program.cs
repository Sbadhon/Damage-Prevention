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

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.WebHost.UseUrls("http://localhost:5108");

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

        builder.Services.AddSingleton<IDateTime, SystemClock>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantBehavior<,>));

        builder.Services.AddSingleton<IWorkOrderRepository, InMemoryWorkOrderRepository>();

        builder.Services.Configure<TicketEventsOptions>(
            builder.Configuration.GetSection("TicketEvents"));

        builder.Services.AddHostedService<TicketSubmittedEventProcessor>();

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
