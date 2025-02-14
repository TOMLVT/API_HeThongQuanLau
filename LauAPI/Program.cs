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
    options.ListenAnyIP(8080); // API s? ch?y tr�n c?ng 8080
    options.ListenAnyIP(8081); // N?u b?n mu?n th�m c?ng 8081
});

var app = builder.Build();

// S? d?ng CORS
app.UseCors("AllowAllOrigins");

// Middleware
app.UseAuthorization();
app.MapControllers();

// Ch?y ?ng d?ng
app.Run();
