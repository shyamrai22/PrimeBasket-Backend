using Microsoft.EntityFrameworkCore;
using PrimeBasket.Auth.API.Data;
using PrimeBasket.Auth.API.Interfaces.Auth;
using PrimeBasket.Auth.API.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<TokenService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PrimeBasket Auth API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token directly here (Swagger will add 'Bearer' for you)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PrimeBasket Auth API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any(u => u.Role == "Admin"))
    {
        var hasher = scope.ServiceProvider.GetRequiredService<PrimeBasket.Auth.API.Services.Auth.PasswordHasher>();
        db.Users.Add(new PrimeBasket.Auth.API.Entities.User
        {
            FullName = "System Admin",
            Email = "admin@primebasket.com",
            PasswordHash = hasher.Hash("Admin@123"),
            Role = "Admin",
            Status = "Approved"
        });
        db.SaveChanges();
    }

    if (!db.Users.Any(u => u.Role == "Merchant"))
    {
        var hasher = scope.ServiceProvider.GetRequiredService<PrimeBasket.Auth.API.Services.Auth.PasswordHasher>();
        db.Users.Add(new PrimeBasket.Auth.API.Entities.User
        {
            FullName = "Premium Merchant",
            Email = "merchant@primebasket.com",
            PasswordHash = hasher.Hash("Merchant@123"),
            Role = "Merchant",
            Status = "Approved",
            BusinessName = "Premium Electronics",
            BusinessType = "Retail",
            StoreDescription = "Top tier electronics store."
        });
        db.SaveChanges();
    }
}

app.Run();

