var builder = WebApplication.CreateBuilder(args);

// C?u h�nh Dependency Injection
builder.Services.AddSingleton<SqlDataAccess>();

// C?u h�nh CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()  // Cho ph�p t?t c? c�c ngu?n (origins)
              .AllowAnyMethod()  // Cho ph�p t?t c? c�c ph??ng th?c HTTP (GET, POST, PUT, DELETE...)
              .AllowAnyHeader(); // Cho ph�p t?t c? c�c headers
    });
});

// Th�m d?ch v? API
builder.Services.AddControllers();

var app = builder.Build();

// S? d?ng CORS
app.UseCors("AllowAllOrigins");

// C?u h�nh c�c middleware v� c�c route
app.UseAuthorization();
app.MapControllers();

app.Run();
