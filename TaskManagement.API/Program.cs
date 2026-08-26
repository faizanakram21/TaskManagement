using FluentValidation;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Facebook;  // 👈 add
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using TaskManagement.Application.Behaviors;
using TaskManagement.Application.Features.Tasks.Commands.CreateTask;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;

// =============================================
// 🪵 SERILOG CONFIGURATION
// =============================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// =============================================
// 🪵 SERILOG
// =============================================
builder.Host.UseSerilog();

// =============================================
// 🔥 FIREBASE INITIALIZE
// =============================================
// =============================================
// 🔥 FIREBASE INITIALIZE (optional — skip if credentials missing, e.g. in CI)
// =============================================
var firebaseCredPath = builder.Configuration["Firebase:CredentialPath"];
if (!string.IsNullOrEmpty(firebaseCredPath) && File.Exists(firebaseCredPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseCredPath)
    });
}
else
{
    Log.Warning("⚠️ Firebase credentials not found at '{Path}'. Skipping Firebase initialization.", firebaseCredPath);
}

// =============================================
// 🗄️ DATABASE
// =============================================
builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// =============================================
// 💉 DEPENDENCY INJECTION
// =============================================
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();   // ✅ nayi line
//builder.Services.AddScoped<IEmailService, EmailService>();
//builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

// =============================================
// ✅ FLUENT VALIDATION
// =============================================
builder.Services.AddValidatorsFromAssembly(
    typeof(CreateTaskCommand).Assembly);

// =============================================
// 📨 MEDIATR
// =============================================
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateTaskCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// =============================================
// 🔐 JWT + FACEBOOK AUTHENTICATION
// =============================================
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    })
    .AddFacebook(opt =>                                           // 👈 add
    {
        opt.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
        opt.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
    });

builder.Services.AddAuthorization();

// =============================================
// 🌐 CORS
// =============================================
builder.Services.AddCors(opt =>
    opt.AddPolicy("Angular", p =>
        p.WithOrigins("http://localhost:4200")
         .AllowAnyMethod()
         .AllowAnyHeader()));

// =============================================
// 🔧 CONTROLLERS + SWAGGER
// =============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =============================================
// 🪵 SERILOG REQUEST LOGGING
// =============================================
app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.0000}ms)";
});

// =============================================
// ⚙️ MIDDLEWARE PIPELINE
// =============================================
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// =============================================
// 🗄️ AUTO MIGRATION
// =============================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// =============================================
// 🚀 RUN
// =============================================
try
{
    Log.Information("🚀 TaskManagement API starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application failed to start!");
}
finally
{
    Log.CloseAndFlush();
}