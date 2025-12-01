using Amazon;
using Amazon.DynamoDBv2;
using Swashbuckle.AspNetCore.SwaggerGen;
using PartialBookSalesScoring.Repositories;
using PartialBookSalesScoring.Services;


var builder = WebApplication.CreateBuilder(args);

// Configuration: weights can be set in appsettings or as environment variables
builder.Configuration.AddJsonFile("appsettings.json", optional: true)
                     .AddEnvironmentVariables();

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Scoring service
builder.Services.Configure<ScoringOptions>(builder.Configuration.GetSection("ScoringOptions"));
builder.Services.AddSingleton<IScoringService, ScoringService>();

// Optionally wire DynamoDB repository (if AWS credentials provided and table exists)
var useDynamo = builder.Configuration.GetValue<bool>("UseDynamoDb", false);
if (useDynamo)
{
    var awsRegion = builder.Configuration.GetValue<string>("AWS_REGION") ?? "us-east-1";
    builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
    {
        // The SDK will pick up credentials from env/EC2/IAM role if available
        return new AmazonDynamoDBClient(Amazon.RegionEndpoint.GetBySystemName(awsRegion));
    });
    builder.Services.AddSingleton<IAccountRepository, DynamoAccountRepository>();
}
else
{
    builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

/* var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
*/

