using Evernest.API.Hubs;
using Evernest.API.Middlewares;
using Evernest.API.Services;
using Evernest.API.Services.Interfaces;
using Evernest.API.Configuration;
using Evernest.Repository.Repositories;
using Evernest.Repository.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/evernest-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Evernest API", Version = "v1" });
    
    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Ignore problematic types for Swagger generation
    c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "DefaultSecretKey12345678901234567890";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5173", "http://localhost:5174", "http://localhost:5177")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetPreflightMaxAge(TimeSpan.FromDays(1));
    });
});

// Configure Firebase - Disabled for development, using in-memory admin codes instead
// var credentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "evernest-project-firebase-adminsdk-fbsvc-637ff31221.json");
// if (File.Exists(credentialPath))
// {
//     var credential = GoogleCredential.FromFile(credentialPath);
//     
//     // Create FirestoreDb with credentials
//     var firestoreDbBuilder = new FirestoreDbBuilder
//     {
//         ProjectId = "evernest-project",
//         Credential = credential
//     };
//     builder.Services.AddSingleton(firestoreDbBuilder.Build());
//     
//     if (FirebaseApp.DefaultInstance == null)
//     {
//         FirebaseApp.Create(new AppOptions
//         {
//             Credential = credential,
//             ProjectId = "evernest-project"
//         });
//     }
// }
// else
// {
//     // For development, use environment variable or default credentials
//     Console.WriteLine("Firebase credentials file not found. Please ensure the credentials file is in the output directory.");
//     throw new FileNotFoundException("Firebase credentials file not found", credentialPath);
// }

// Use in-memory admin code service for development
builder.Services.AddSingleton<IAdminCodeService, InMemoryAdminCodeService>();

// Configure admin code requirement
builder.Services.Configure<AdminCodeSettings>(builder.Configuration.GetSection("AdminCodeSettings"));
builder.Services.AddSingleton(new AdminCodeSettings { RequireAdminCode = false }); // Set to true to require admin code

// Register repositories - Using in-memory for development
builder.Services.AddScoped<IUserRepository, InMemoryUserRepository>();
builder.Services.AddScoped<IFriendRequestRepository, InMemoryFriendRequestRepository>();
builder.Services.AddScoped<IChatRepository, InMemoryChatRepository>();
builder.Services.AddScoped<IMessageRepository, InMemoryMessageRepository>();
builder.Services.AddScoped<IEventRepository, InMemoryEventRepository>();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Add SignalR
builder.Services.AddSignalR();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Evernest API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Handle preflight requests
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204;
        context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        return;
    }
    await next();
});

// Custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub
app.MapHub<ChatHub>("/chathub");

// Seed default admin code
using (var scope = app.Services.CreateScope())
{
    var adminCodeService = scope.ServiceProvider.GetRequiredService<IAdminCodeService>();
    await adminCodeService.SeedDefaultAdminCodeAsync();
    Log.Information("Default admin code seeded (123456) - In-Memory Mode");
}

try
{
    Log.Information("Starting Evernest API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
