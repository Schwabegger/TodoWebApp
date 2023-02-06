using Authentication.Configuration;
using Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Api.Endpoints;
using Core.Interfaces;
using Core.Services;
using Core.Service;
using Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Update the JWT config from the settings
builder.Services.Configure<JwtConfig>(builder.Configuration.AddJsonFile("appsettings.json", false).Build().GetSection("JwtConfig"));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<TodoDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!));

// Getting the secret from the config
// should later be stored in System variables so the secret is not contained in the source code (more secure)
// (dotnet add user-setting JwtConfig:Secret [SECRET])
// save secret on PC. Also works on azure
var key = Encoding.ASCII.GetBytes(builder.Configuration.AddJsonFile("appsettings.json", false).Build()["JwtConfig:Secret"]!);

var tokenValidationParameters = new TokenValidationParameters
{
    // Validates the JWT with the secret so not every JWT works, only the ones WE issued
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false, //TODO: Update
    ValidateAudience = false, //TODO: Update
    RequireExpirationTime = false, //TODO: Update
    ValidateLifetime = true
};
// Injecting into our DI container
builder.Services.AddSingleton(tokenValidationParameters);

builder.Services
    .AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = tokenValidationParameters;
    });
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TodoDbContext>();

builder.Services.AddAuthorization();
    //options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
//    options.AddPolicy("UserOnly", policy => policy.RequireRole("user"));
//});

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<ITodoUserService, TodoUserService>();
builder.Services.AddScoped<ITodoTaskService, TodoTaskService>();

var app = builder.Build();

//Migrate DB on startup
var scopeToUseInProgramCs = app.Services.CreateScope();
var db = scopeToUseInProgramCs.ServiceProvider.GetRequiredService<TodoDbContext>();
db.Database.Migrate();

// add default roles
await RolesData.SeedRoles(scopeToUseInProgramCs.ServiceProvider);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureTodoEndpoints();
app.ConfigureAuthEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();

public static class RolesData
{
    private static readonly string[] Roles = new string[] { "admin", "user" };

    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var dbContext = serviceScope.ServiceProvider.GetService<TodoDbContext>();

            if (!dbContext.UserRoles.Any())
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                foreach (var role in Roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
        }
    }
}