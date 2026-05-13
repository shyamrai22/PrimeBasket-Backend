using Microsoft.EntityFrameworkCore;
using PrimeBasket.Product.API.Data;
using PrimeBasket.Product.API.Interfaces;
using PrimeBasket.Product.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT
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
        IssuerSigningKey = new SymmetricSecurityKey(key),

        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
});

// Services
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PrimeBasket Product API",
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

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API V1");
    c.RoutePrefix = string.Empty;
});

// IMPORTANT ORDER
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.Migrate();

    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new PrimeBasket.Product.API.Entities.Product
            {
                Name = "MacBook Pro M3",
                Description = "Latest Apple MacBook Pro with M3 chip, 16GB RAM, 512GB SSD.",
                Price = 1999.99m,
                Stock = 25,
                Category = "Laptops",
                MerchantId = 2, // Corresponds to the seeded Premium Merchant
                ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=800&q=80",
                Status = "Active"
            },
            new PrimeBasket.Product.API.Entities.Product
            {
                Name = "Sony WH-1000XM5",
                Description = "Industry leading noise canceling wireless headphones.",
                Price = 348.00m,
                Stock = 50,
                Category = "Audio",
                MerchantId = 2,
                ImageUrl = "https://images.unsplash.com/photo-1618366712010-f4ae9c647dcb?auto=format&fit=crop&w=800&q=80",
                Status = "Active"
            },
            new PrimeBasket.Product.API.Entities.Product
            {
                Name = "Samsung Galaxy S24 Ultra",
                Description = "Samsung's flagship smartphone with Galaxy AI features.",
                Price = 1299.00m,
                Stock = 30,
                Category = "Smartphones",
                MerchantId = 2,
                ImageUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?auto=format&fit=crop&w=800&q=80",
                Status = "Active"
            },
            new PrimeBasket.Product.API.Entities.Product
            {
                Name = "Dell UltraSharp 27 4K Monitor",
                Description = "Brilliant 4K monitor with amazing color accuracy.",
                Price = 599.99m,
                Stock = 15,
                Category = "Monitors",
                MerchantId = 2,
                ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=800&q=80",
                Status = "Active"
            }
        );
        db.SaveChanges();
    }
}

app.Run();

