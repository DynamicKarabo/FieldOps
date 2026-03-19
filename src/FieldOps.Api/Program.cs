using FieldOps.Application.Common.Behaviors;
using FieldOps.Application.Jobs.Commands;
using FieldOps.Application.Realtime;
using FieldOps.Application.Sla;
using FieldOps.Infrastructure.Persistence;
using FieldOps.Infrastructure.Realtime;
using FieldOps.Infrastructure.Sla;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string \"DefaultConnection\" is missing.");
}

builder.Services.AddDbContext<FieldOpsDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IFieldOpsDbContext>(provider => provider.GetRequiredService<FieldOpsDbContext>());

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateJobCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateJobCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<ISlaCalculator, SlaCalculator>();
builder.Services.AddScoped<IJobNotificationService, SignalRNotifier>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHub<JobHub>("/hubs/jobs");

app.Run();
