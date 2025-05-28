// EduSync/Program.cs (Backend Project)

using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Settings; // Using your 'Configuration' folder for EmailSettings
using EduSync.Services; 

var builder = WebApplication.CreateBuilder(args);

// Define a specific CORS policy name
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; 

// 1. Configure DbContext (Existing code)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EduSyncDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions => // Added SQL Server options for resiliency
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5, 
            maxRetryDelay: TimeSpan.FromSeconds(30), 
            errorNumbersToAdd: null); 
    }));


// 2. Add CORS services and define a policy (Existing code)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173") // Your local frontend's origin
                                // TODO: Add your deployed frontend's URL here later for Azure Static Web App
                                // .WithOrigins("https://YOUR_STATIC_WEB_APP_URL.azurestaticapps.net") 
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

// Add services to the container.
// Configure EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register IEmailService and its implementation
builder.Services.AddTransient<IEmailService, SendGridEmailService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Always enable Swagger and Swagger UI for now for easier testing on Azure.
// In a production scenario, you might want to conditionally enable this
// or protect the Swagger endpoint.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduSync API V1");
    // If deploying to a virtual directory or path, you might need:
    // string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
    // c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "EduSync API V1");
});


app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization(); // Ensure this is after UseCors

app.MapControllers();

app.Run();
