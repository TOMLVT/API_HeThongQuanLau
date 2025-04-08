var builder = WebApplication.CreateBuilder(args);

// C?u hình Dependency Injection
builder.Services.AddSingleton<SqlDataAccess>();

// C?u hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Thêm d?ch v? API
builder.Services.AddControllers();

// C?u hình Kestrel ?? l?ng nghe trên t?t c? ??a ch? IP
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5137);
  //  options.ListenAnyIP(8081);
});

// Logging rõ ràng h?n khi ch?y trong Docker
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Middleware
app.UseCors("AllowAllOrigins");
app.UseAuthorization();
app.MapControllers();

app.Run();
