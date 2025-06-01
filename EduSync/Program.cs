// EduSync/Program.cs (Backend Project)

using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Settings; // For EmailSettings and EventHubSettings
using EduSync.Services;
using EduSync.Configuration;
// If EventHubSettings.cs is in a 'Configuration' folder, use:
 using EduSync.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Define a specific CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// 1. Configure DbContext (Existing code)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EduSyncDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));


// 2. Add CORS services and define a policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(
                                    "http://localhost:5173", // Your local frontend's origin
                                    "https://kind-mud-0c1cc5000.6.azurestaticapps.net" // Your deployed frontend URL
                                 )
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Add services to the container.
// Configure EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure EventHubSettings
builder.Services.Configure<EventHubSettings>(builder.Configuration.GetSection("EventHubSettings"));

// Register IEmailService and its implementation
builder.Services.AddTransient<IEmailService, SendGridEmailService>();

// Register IEventHubService and its implementation  <-- NEWLY ADDED
// EventHubProducerClient is thread-safe and intended to be long-lived.
// A singleton lifetime for EventHubService (which holds the producer client) is appropriate.
builder.Services.AddSingleton<IEventHubService, EventHubService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appInsightsKey = builder.Configuration["ApplicationInsights:InstrumentationKey"];
if (!string.IsNullOrEmpty(appInsightsKey))
{
    builder.Services.AddApplicationInsightsTelemetry(appInsightsKey);
}


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduSync API V1");
});


app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
