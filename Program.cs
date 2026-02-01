using HseBackend.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configure Port for Railway/PaaS
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use SQLite with explicit relative path
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "hse.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<HseBackend.Services.PdfService>();
builder.Services.AddScoped<HseBackend.Services.EmailService>();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    DbSeeder.SeedQuestions(dbContext);
    // DbSeeder.SeedUsers(dbContext); // Disabled as per user request for "real data only"
}

app.UseCors("AllowAll");
app.UseStaticFiles(); // Enable serving generated PDFs
app.UseAuthorization();
app.MapControllers();

QuestPDF.Settings.License = LicenseType.Community;

// Register Cairo Fonts for Arabic Support
var fontDir = Path.Combine(app.Environment.WebRootPath, "fonts");
var regularFont = Path.Combine(fontDir, "Cairo-Regular.ttf");
var boldFont = Path.Combine(fontDir, "Cairo-Bold.ttf");

if (File.Exists(regularFont))
{
    FontManager.RegisterFont(File.OpenRead(regularFont));
}
if (File.Exists(boldFont))
{
    FontManager.RegisterFont(File.OpenRead(boldFont));
}

app.Run();
