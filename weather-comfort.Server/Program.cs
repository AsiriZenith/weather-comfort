using weather_comfort.Server.Infrastructure;
using weather_comfort.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register IMemoryCache for caching weather data
builder.Services.AddMemoryCache();

// Register CityDataService as Singleton since city data doesn't change at runtime
builder.Services.AddSingleton<ICityDataService, CityDataService>();

// Register HTTP client for OpenWeatherMap API
builder.Services.AddHttpClient<IOpenWeatherClient, OpenWeatherClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); 
});

// Register WeatherService as Scoped
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Register ComfortIndexService as Scoped
builder.Services.AddScoped<IComfortIndexService, ComfortIndexService>();

// Register RankingService as Scoped
builder.Services.AddScoped<IRankingService, RankingService>();

// Configure CORS to allow requests from the Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("https://127.0.0.1:5899", "https://localhost:5899")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS - must be before UseAuthorization and MapControllers
app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
