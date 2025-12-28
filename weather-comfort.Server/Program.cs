using weather_comfort.Server.Infrastructure;
using weather_comfort.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
