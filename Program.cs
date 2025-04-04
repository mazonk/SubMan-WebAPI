using DotNetEnv;
using Subman.Database;
using Subman.Repositories;
using Subman.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Load env variables
Env.Load();
var mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
if (string.IsNullOrEmpty(mongoDbConnectionString))
{
    Console.WriteLine("MongoDB connection string is not set in environment variables.");
}
else
{
    Console.WriteLine("MongoDB connection string: " + mongoDbConnectionString);
}


// Add environment variables to the configuration
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => 
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

builder.Services.AddScoped<SubscriptionRepository>();
builder.Services.AddScoped<UserRepository>();

builder.Services.AddSingleton<CronJobService>();

// Add MongoDB connection service (singleton)
builder.Services.AddSingleton<MongoDbContext>(sp =>
    new MongoDbContext(mongoDbConnectionString)
);

// Add Swagger services
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Subman API",
        Version = "v1",
        Description = "A simple API to manage subscriptions"
    });

    // Enable XML comments
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "SubscriptionManager.xml");
    options.IncludeXmlComments(xmlFile);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = "localhost",
            ValidAudience = "localhost",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"]!))
        };
    });

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
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
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization();  // To use authorization


var app = builder.Build();

// Enable CORS
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Subscription manager");
    options.RoutePrefix = string.Empty;
});

// Map root
app.MapGet("/", () => Results.Ok("SubMan API is running 🚀"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
