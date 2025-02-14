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
    options.ListenAnyIP(8080); // API s? ch?y trên c?ng 8080
    options.ListenAnyIP(8081); // N?u b?n mu?n thêm c?ng 8081
});

var app = builder.Build();

// S? d?ng CORS
app.UseCors("AllowAllOrigins");

// Middleware
app.UseAuthorization();
app.MapControllers();

// Ch?y ?ng d?ng
app.Run();
