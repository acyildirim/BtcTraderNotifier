using BtcTrader.Core.MappingProfiles;
using BtcTrader.Core.Services.Instructions.Abstractions;
using BtcTrader.Core.Services.Instructions.Implementations;
using BtcTrader.Core.Services.Notification.Abstractions;
using BtcTrader.Core.Services.Notification.Implementations;
using BtcTrader.Core.Services.User.Abstractions;
using BtcTrader.Core.Services.User.Implementations;
using BtcTrader.Data;
using BtcTrader.Data.Repositories.Implementations;
using BtcTrader.Data.Repositories.Interfaces;
using BtcTrader.Domain.Instructions;
using BtcTrader.Domain.User;
using BtcTrader.Extensions.Configurations;
using BtcTrader.Infrastructure.Core;
using BtcTrader.Infrastructure.Services.Abstractions;
using BtcTrader.Infrastructure.Services.Implementations;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
// Add services to the container.
var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Console.WriteLine($"Environment Name : {environmentName}");
var configuration = builder.Configuration;


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1,0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("x-api-version"));
});

services.AddAutoMapper(typeof(InstructionMappingProfiles).Assembly,typeof(UserMappingProfiles).Assembly);
services.AddScoped(typeof(IExpressionBuilderService<Instruction>), typeof(InstructionExpressionBuilder));
services.AddScoped(typeof(IExpressionBuilderService<InstructionAudit>), typeof(InstructionAuditExpressionBuilder));
services.AddScoped(typeof(IQueryService<>), typeof(QueryService<>));
services.AddIdentityCore<AppUser>(opt =>
    {
        opt.Password.RequireNonAlphanumeric = false;

    })
    .AddEntityFrameworkStores<ApplicationDbContext>();
services.AddScoped(typeof(IInstructionsService), typeof(InstructionsService));
services.AddScoped(typeof(IUserService), typeof(UserService));
Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));
services.AddHangfire(config =>
    config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

services.AddScoped<IEmailService, EmailService>();

MassTransitConfiguration.AddMassTransitServices(services,configuration);

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Hangfire Settings
app.UseHangfireServer();
app.UseHangfireDashboard();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var scopeServices = scope.ServiceProvider;

    try
    {
        var context = scopeServices.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();    

    }
    catch (Exception ex)
    {
        var logger = scopeServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occured during migration");                
    }
   
}

await app.RunAsync();



