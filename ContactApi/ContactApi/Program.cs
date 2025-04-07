using ContactApi.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// --- Configure Services ---
// Add services required by the application to the dependency injection (DI) container.

#region CORS Configuration
// Define a name for the CORS policy for easy reference.
var MyAllowSpecificOrigins = "_myAllowSpecificOriginsPolicy";

// Add CORS services to the DI container.
builder.Services.AddCors(options =>
{
    // Define a named CORS policy.
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // Specify the origin(s) allowed to make requests.
                          // MUST exactly match the frontend URL (scheme, host, port).
                          policy.WithOrigins("http://localhost:4200")
                                // Allow any HTTP header to be sent in the request.
                                .AllowAnyHeader()
                                // Allow any standard HTTP method (GET, POST, PUT, DELETE, OPTIONS, etc.).
                                // OPTIONS is needed for preflight requests before POST/PUT/DELETE with certain headers.
                                .AllowAnyMethod();
                      });
});
#endregion

#region Application Services
// Add services needed for MVC Controllers.
builder.Services.AddControllers();

// Register custom services for Dependency Injection.
// IContactRepository is the interface, ContactRepository is the implementation.
// AddScoped creates one instance per HTTP request.
builder.Services.AddScoped<IContactRepository, ContactRepository>();

// Add services for Swagger/OpenAPI documentation generation and UI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Contact API", Version = "v1" });
});

// Add logging services.
builder.Services.AddLogging();
#endregion

// Build the application host.
var app = builder.Build();

// --- Configure the HTTP Request Pipeline (Middleware) ---
// Define how incoming HTTP requests are processed. Order matters significantly.

#region Environment-Specific Configuration
// Configure middleware differently for Development vs. Production environments.
if (app.Environment.IsDevelopment())
{
    // Enable Swagger middleware to serve the generated JSON spec.
    app.UseSwagger();
    // Enable Swagger UI middleware to provide an interactive documentation page.
    app.UseSwaggerUI();
    // Use detailed exception page for easier debugging during development.
    app.UseDeveloperExceptionPage();
}
else
{
    // Use a generic exception handler for production (redirects or logs).
    app.UseExceptionHandler("/Error");
    // Use HTTP Strict Transport Security (HSTS) for enhanced security in production.
    app.UseHsts();
}
#endregion

#region Standard Middleware Pipeline
// 1. Redirect HTTP requests to HTTPS. Enforces secure connection.
app.UseHttpsRedirection();

// 2. Enable endpoint routing. Matches request URL to application endpoints.
// Must come before middleware that rely on routing information (CORS, Auth, Endpoints).
app.UseRouting();

// 3. Apply the configured CORS policy. Allows requests from the configured origins.
// Must be placed after UseRouting and before UseAuthorization/MapControllers.
app.UseCors(MyAllowSpecificOrigins);

// 4. Enable authorization middleware. Applies authorization policies.
app.UseAuthorization();

// 5. Map controller routes. Executes the matched controller actions.
app.MapControllers();
#endregion

// Start the application and listen for requests.
app.Run();