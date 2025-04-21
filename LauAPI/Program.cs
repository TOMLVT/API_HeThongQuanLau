using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// C?u hình Injection ------------------------------------------------------

builder.Services.AddSingleton<SqlDataAccess>();
builder.Services.AddSingleton<DishGroupDataAccess>();
builder.Services.AddSingleton<SqlMonAnAccess>();

// ------------------------------------------------------
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
    // options.ListenAnyIP(8081);
});

// Logging rõ ràng h?n khi ch?y trong Docker
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Middleware
app.UseCors("AllowAllOrigins");


// C?u hình l?y ?nh t? server l?u tr? -------------------------
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HeThongQuanLau")),
    RequestPath = ""
});


// C?u hình ph?c v? t?p t?nh t? th? m?c wwwroot/images ---------------------------------------------------------
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")), // ???ng d?n ??n th? m?c ch?a ?nh
    RequestPath = "/images"  // ??m b?o ?nh s? ???c truy c?p qua /images/{tên ?nh}
});


app.UseAuthorization();
app.MapControllers();

app.Run();
