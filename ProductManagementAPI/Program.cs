using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductManagementAPI.Data;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký controller
builder.Services.AddControllers();

// Đăng ký OpenAPI (.NET 10)
builder.Services.AddOpenApi();

// Kết nối SQL Server
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ProductDatabase")));

// Đọc cấu hình JWT từ appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Key");

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Cấu hình xác thực JWT tương thích với Bài 2 (JwtApi)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Tự động kiểm tra và khởi tạo CSDL nếu chưa có
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    dbContext.Database.EnsureCreated();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Không thể tự động kết nối CSDL: {Message}", ex.Message);
}

// Bật giao diện OpenAPI & Scalar UI để xem và thử nghiệm API trên trình duyệt
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Trang chủ hiển thị trạng thái API thay vì bị lỗi 404
app.MapGet("/", () => Results.Ok(new
{
    status = "Online",
    message = "ProductManagementAPI đang chạy thành công!",
    docs = "/scalar/v1",
    endpoints = new[]
    {
        "GET /products (Yêu cầu JWT Token)",
        "GET /products/{id} (Yêu cầu JWT Token)",
        "POST /products (Yêu cầu JWT Token)",
        "PUT /products/{id} (Yêu cầu JWT Token)",
        "DELETE /products/{id} (Yêu cầu JWT Token)"
    }
}));

// Tắt HttpsRedirection để tránh lỗi chuyển hướng 307 khi test Postman với HTTP
// app.UseHttpsRedirection();

// Authentication phải chạy trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();