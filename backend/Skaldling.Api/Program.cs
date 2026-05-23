using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Skaldling.Api.Features.Heroes.CreateHero;
using Skaldling.Api.Features.Heroes.GetHero;
using Skaldling.Api.Features.Heroes.ListHeroes;
using Skaldling.Api.Features.Heroes.UpdateAvatar;
using Skaldling.Api.Features.Sprites.ListSprites;
using Skaldling.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register OpenAPI for Scalar to use
builder.Services.AddOpenApi();

// Register EF Core / Postgres
builder.Services.AddDbContext<SkaldlingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("SkaldlingDb")
        ?? throw new InvalidOperationException("Connection string 'SkaldlingDb' not found.")));

// Register TimeProvider (testable timestamps)
builder.Services.AddSingleton(TimeProvider.System);

// Register FluentValidation (scan assembly for all validators)
builder.Services.AddValidatorsFromAssemblyContaining<CreateHeroValidator>();

// Register feature handlers
builder.Services.AddScoped<CreateHeroHandler>();
builder.Services.AddScoped<GetHeroHandler>();
builder.Services.AddScoped<ListHeroesHandler>();
builder.Services.AddScoped<UpdateAvatarHandler>();
builder.Services.AddScoped<ListSpritesHandler>();

// SvelteKit CORS registration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Expose Scalar UI + OpenAPI endpoints when running in dev
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("DevFrontend");

// Apply any pending migrations on startup to keep database updated - TO BE REMOVED OUT OF DEV!
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SkaldlingDbContext>();
    db.Database.Migrate();
}

// Map feature endpoints
app.MapCreateHero();
app.MapGetHero();
app.MapListHeroes();
app.MapUpdateAvatar();
app.MapListSprites();

app.Run();
