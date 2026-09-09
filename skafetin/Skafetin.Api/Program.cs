using Skafetin.Api.Ai;
using Skafetin.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Skafetin.Api.Security;
using Skafetin.Api.Storage;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SkafetinDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("Nedostaje Jwt konfiguracija.");

if (jwtOptions.SigningKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:SigningKey nije postavljen ili je kraći od 32 znaka. " +
        "Postavi ga naredbom: dotnet user-secrets set \"Jwt:SigningKey\" \"<ključ>\" " +
        "u mapi projekta Skafetin.Api.");

builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddScoped<JwtTokenService>();

builder.Services.Configure<AiOptions>(builder.Configuration.GetSection(AiOptions.SectionName));
builder.Services.AddScoped<MockAiService>();
builder.Services.AddScoped<OpenAiService>();

// Kontroler zna samo za IAiService; konfiguracija odlucuje koja ga klasa izvrsava.
// Nepoznat provider ili neispravna konfiguracija vracaju se na mock uz upozorenje,
// jer je bolje da aplikacija radi s lokalnim generatorom nego da ne krene.
builder.Services.AddScoped<IAiService>(services =>
{
    var provider = builder.Configuration[$"{AiOptions.SectionName}:Provider"];
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Ai");

    if (string.IsNullOrWhiteSpace(provider)
        || string.Equals(provider, AiOptions.MockProvider, StringComparison.OrdinalIgnoreCase))
        return services.GetRequiredService<MockAiService>();

    if (string.Equals(provider, AiOptions.OpenAiProvider, StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            return services.GetRequiredService<OpenAiService>();
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(
                exception,
                "Provider {Provider} nije se mogao stvoriti. Koristi se {Fallback}.",
                provider,
                AiOptions.MockProvider);

            return services.GetRequiredService<MockAiService>();
        }
    }

    logger.LogWarning(
        "Ai:Provider je postavljen na '{Provider}', za koji ne postoji implementacija. Koristi se {Fallback}.",
        provider,
        AiOptions.MockProvider);

    return services.GetRequiredService<MockAiService>();
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy => policy.RequireRole("Admin"));

    options.AddPolicy(
        AuthorizationPolicies.Manage,
        policy => policy.RequireRole("Admin", "InventoryManager"));

    options.AddPolicy(
        AuthorizationPolicies.InventoryWork,
        policy => policy.RequireRole("Admin", "InventoryManager", "LocationResponsible"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SkafetinDbContext>();
    await db.Database.MigrateAsync();

    var seedLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

    await SeedData.SeedAsync(
        db,
        seedLogger,
        MediaStorage.GetSeedFilesDirectory(app.Environment.ContentRootPath),
        MediaStorage.GetUploadDirectory(app.Configuration, app.Environment.ContentRootPath));
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
