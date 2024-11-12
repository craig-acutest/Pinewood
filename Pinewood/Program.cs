using Serilog;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using Pinewood.Services;
using Microsoft.Extensions.DependencyInjection;
using Pinewood.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console() // Logs to the console
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, shared: true) // Logs to a file
    .CreateLogger();

builder.Host.UseSerilog();
builder.Logging.AddConsole();

// Insert data into the "healthcheck" table when the application starts
// can be replaced with a health check url, and logged in app insights or similar, to check the health of the application
using (IDbConnection db = new SqlConnection(connectionString))
{
    Log.Information("Starting up dapper, write to test table.");
    Log.Information($"Using Connection String: {connectionString}");
    try
    {
        var sql = "INSERT INTO healthcheck (Name, Date) VALUES (@Name, @Date)";
        var parameters = new { Name = "Pinewood", Date = DateTime.Now.ToString("dd/MM/yyyy HH:mm") };
        db.Execute(sql, parameters);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error inserting data into the test table.");
    }
}

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddHttpClient<CustomerApiClient>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<AuthenticatedHandler>();

builder.Services.AddHttpClient<CustomerApiClient>()
.AddHttpMessageHandler<AuthenticatedHandler>();

builder.Services.AddHttpClient<AuthApiClient>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
});

try
{
    Log.Information("Starting up the application");
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<VisitorMiddleware>();

    app.Use(async (context, next) =>
    {
        await next();

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            context.Response.Redirect("/Home/Unauthorized");
        }
    });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();

    Log.Information("Application started successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "The application failed to start");
}
finally
{
    Log.CloseAndFlush();
}