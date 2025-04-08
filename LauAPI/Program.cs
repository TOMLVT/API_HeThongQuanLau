var builder = WebApplication.CreateBuilder(args);

// C?u h�nh Dependency Injection
builder.Services.AddSingleton<SqlDataAccess>();

// C?u h�nh CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Th�m d?ch v? API
builder.Services.AddControllers();

// C?u h�nh Kestrel ?? l?ng nghe tr�n t?t c? ??a ch? IP
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5137);
  //  options.ListenAnyIP(8081);
});

// Logging r� r�ng h?n khi ch?y trong Docker
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Middleware
app.UseCors("AllowAllOrigins");
app.UseAuthorization();
app.MapControllers();

app.Run();
