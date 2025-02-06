var builder = WebApplication.CreateBuilder(args);

// C?u hình Dependency Injection
builder.Services.AddSingleton<SqlDataAccess>();

// C?u hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()  // Cho phép t?t c? các ngu?n (origins)
              .AllowAnyMethod()  // Cho phép t?t c? các ph??ng th?c HTTP (GET, POST, PUT, DELETE...)
              .AllowAnyHeader(); // Cho phép t?t c? các headers
    });
});

// Thêm d?ch v? API
builder.Services.AddControllers();

var app = builder.Build();

// S? d?ng CORS
app.UseCors("AllowAllOrigins");

// C?u hình các middleware và các route
app.UseAuthorization();
app.MapControllers();

app.Run();
