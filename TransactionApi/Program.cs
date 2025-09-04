using log4net;
using log4net.Config;
using TransactionApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Enable log4net
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddLog4Net("log4net.config");

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

builder.Services.AddSingleton<PartnerAuthService>();
builder.Services.AddSingleton<SignatureService>();
builder.Services.AddSingleton<DiscountService>();

var app = builder.Build();

app.UseCors("AllowAll");

// Swagger always ON
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transaction API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
