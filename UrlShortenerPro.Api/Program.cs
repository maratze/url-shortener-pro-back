using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UrlShortenerPro.Core.Interfaces;
using UrlShortenerPro.Core.Services;
using UrlShortenerPro.Infrastructure.Data;
using UrlShortenerPro.Infrastructure.Interfaces;
using UrlShortenerPro.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "URL Shortener Pro API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
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
            Array.Empty<string>()
        }
    });
});

// Добавление конфигурации БД с PostgreSQL вместо SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление репозиториев
builder.Services.AddScoped<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IClickDataRepository, ClickDataRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Добавление сервисов
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IUserService, UserService>();

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Настройка JWT аутентификации
var jwtKey = builder.Configuration["JwtSettings:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    jwtKey = "XLMwk2Xg7oKpuBXQLXqv9zWyXMrgUUEoul5jwlXpDD4=";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "UrlShortenerPro",
            ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "UrlShortenerUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Уменьшаем допустимое расхождение времени
        };

        // Добавляем обработку событий для более подробной отладки
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

// Добавляем в Program.cs для диагностики
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();

    // Устанавливаем уровень логирования для Microsoft.AspNetCore.Authentication на Debug
    logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);
});

var app = builder.Build();

// Конфигурация HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Важно: порядок middleware имеет значение!
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ПРАВИЛЬНЫЙ ПОРЯДОК для routing и auth middleware
app.UseRouting(); // Сначала routing
app.UseAuthentication(); // Затем authentication
app.UseAuthorization(); // Затем authorization
app.MapControllers(); // И наконец endpoints

// Применение миграций БД при запуске
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();