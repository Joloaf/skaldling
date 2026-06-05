using System.Globalization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Skaldling.Api.Features.Heroes.CreateHero;
using Skaldling.Api.Features.Heroes.GetHero;
using Skaldling.Api.Features.Heroes.ListHeroes;
using Skaldling.Api.Features.Heroes.UpdateAvatar;
using Skaldling.Api.Features.Heroes.UpdateHeroDetails;
using Skaldling.Api.Features.Sprites.ListSprites;
using Skaldling.Api.Features.Themes.ListThemes;
using Skaldling.Api.Features.Adventures.CreateAdventure;
using Skaldling.Api.Features.Adventures.GenerateStory;
using Skaldling.Api.Features.Adventures.GetAdventure;
using Skaldling.Api.Features.Adventures.StartAdventure;
using Skaldling.Api.Features.Adventures.UpdateTaskCompletion;
using Skaldling.Api.Features.Adventures.ConfirmFinaleReward;
using Skaldling.Api.Features.Dev.PreviewStories;
using Skaldling.Api.Infrastructure.Errors;
using Skaldling.Api.Infrastructure.Persistence;
using Skaldling.Api.Infrastructure.Configuration;
using Skaldling.Api.Infrastructure.StoryGeneration;

var builder = WebApplication.CreateBuilder(args);

// Register OpenAPI for Scalar to use
builder.Services.AddOpenApi();

// Register JSON Serialization to help handle enum strings
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Register EF Core / Postgres
builder.Services.AddDbContext<SkaldlingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("SkaldlingDb")
        ?? throw new InvalidOperationException("Connection string 'SkaldlingDb' not found.")));

// Register TimeProvider (testable timestamps)
builder.Services.AddSingleton(TimeProvider.System);

// Register FluentValidation (scan assembly for all validators)
builder.Services.AddValidatorsFromAssemblyContaining<CreateHeroValidator>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Register LLM integration configuration
builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection("Anthropic"));
builder.Services.Configure<StoryGeneratorOptions>(builder.Configuration.GetSection("StoryGenerator"));

// Register story generation infrastructure
builder.Services.AddScoped<HeroContextAssembler>();
builder.Services.AddScoped<IStoryGenerator>(sp =>
{
    var options = sp.GetRequiredService<IOptions<StoryGeneratorOptions>>().Value;
    return options.Implementation switch
    {
        "Fixture" => ActivatorUtilities.CreateInstance<FixtureStoryGenerator>(sp),
        _ => ActivatorUtilities.CreateInstance<LlmStoryGenerator>(sp)
    };
});

// Register feature handlers
builder.Services.AddScoped<CreateHeroHandler>();
builder.Services.AddScoped<GetHeroHandler>();
builder.Services.AddScoped<ListHeroesHandler>();
builder.Services.AddScoped<UpdateAvatarHandler>();
builder.Services.AddScoped<UpdateHeroDetailsHandler>();
builder.Services.AddScoped<ListSpritesHandler>();
builder.Services.AddScoped<ListThemesHandler>();
builder.Services.AddScoped<CreateAdventureHandler>();
builder.Services.AddScoped<GenerateStoryHandler>();
builder.Services.AddScoped<GetAdventureHandler>();
builder.Services.AddScoped<StartAdventureHandler>();
builder.Services.AddScoped<UpdateTaskCompletionHandler>();
builder.Services.AddScoped<ConfirmFinaleRewardHandler>();
builder.Services.AddScoped<PreviewStoriesHandler>();

// SvelteKit CORS registration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Expose Scalar UI + OpenAPI + Story Preview endpoints when running in dev
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapPreviewStories();
}

app.UseCors("DevFrontend");

app.UseExceptionHandler();
app.UseStatusCodePages();

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
app.MapUpdateHeroDetails();
app.MapListSprites();
app.MapListThemes();
app.MapCreateAdventure();
app.MapGenerateStory();
app.MapGetAdventure();
app.MapStartAdventure();
app.MapUpdateTaskCompletion();
app.MapConfirmFinaleReward();

app.Run();
