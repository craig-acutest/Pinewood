using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pinewood.Api.Data;
using Serilog;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, shared: true)
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
        var parameters = new { Name = "Pinewood.Api", Date = DateTime.Now.ToString("dd/MM/yyyy HH:mm") };
        db.Execute(sql, parameters);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error inserting data into the test table.");
    }
}

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Audience = builder.Configuration["Jwt:Audience"]; // "dotnetapp"
        options.RequireHttpsMetadata = false; // For local development

        // Configure token validation
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // "Pinewood"
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"], // "dotnetapp"
            ValidateLifetime = true, // Ensure the token is not expired
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero // Optional: reduces the allowable clock skew
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                Log.Information("Token validated successfully");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Log.Error("Token validation failed: " + context.Exception.Message);
                // Log detailed error for debugging
                if (context.Exception is SecurityTokenExpiredException)
                {
                    Log.Error("Token has expired.");
                }
                else if (context.Exception is SecurityTokenInvalidIssuerException)
                {
                    Log.Error("Invalid issuer.");
                }
                else if (context.Exception is SecurityTokenInvalidAudienceException)
                {
                    Log.Error("Invalid audience.");
                }
                else if (context.Exception is SecurityTokenSignatureKeyNotFoundException)
                {
                    Log.Error("Signature key not found.");
                }
                else
                {
                    Log.Error("Unhandled token validation exception: " + context.Exception);
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WebOnly", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAssertion(context =>
        {
            var httpContext = context.Resource as HttpContext;
            return httpContext?.Request.Headers["Referer"].ToString().StartsWith("https://web.pinewood.co.uk") == true;
        });
    });
    options.AddPolicy("MauiOnly", policy =>
    {
        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAssertion(context =>
        {
            var httpContext = context.Resource as HttpContext;
            return httpContext?.Request.Headers["Referer"].ToString().StartsWith("https://maui.pinewood.co.uk") == true;
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7193")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

try
{
    Log.Information("Starting up the application");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Apply migrations
        context.Database.Migrate();

        // Seed the database
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        await SeedData.Initialize(roleManager, userManager);
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });

    app.UseHttpsRedirection();

    app.UseCors();

    app.UseAuthorization();
    app.UseAuthentication();

    app.UseSerilogRequestLogging();

    app.MapControllers();

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