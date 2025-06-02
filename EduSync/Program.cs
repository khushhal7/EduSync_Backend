// EduSync/Program.cs (Backend Project)

using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Settings; // For EmailSettings and EventHubSettings
using EduSync.Services;
// If EventHubSettings.cs or BlobStorageSettings (if you create one) are in a 'Configuration' folder, adjust using:
using EduSync.Configuration; // Assuming EmailSettings, EventHubSettings, and a potential BlobStorageSettings might be here or in .Settings

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
// Note: BlobStorageService directly uses IConfiguration for ConnectionString and ContainerName,
// so explicit Configure for a BlobStorageSettings class is not added unless you create one.

// Register IEmailService and its implementation
builder.Services.AddTransient<IEmailService, SendGridEmailService>();

// Register IEventHubService and its implementation
builder.Services.AddSingleton<IEventHubService, EventHubService>();

// Register IBlobStorageService and its implementation  <-- NEWLY ADDED
// BlobServiceClient can be managed as a singleton or created per operation.
// If BlobStorageService creates BlobServiceClient per call or manages it internally for short-lived operations,
// AddTransient or AddScoped might be suitable.
// If BlobStorageService holds a long-lived BlobServiceClient, Singleton would be appropriate.
// Given our current BlobStorageService creates clients per call, AddTransient is fine.
builder.Services.AddTransient<IBlobStorageService, BlobStorageService>();


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
