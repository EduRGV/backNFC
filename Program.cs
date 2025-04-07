using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NFC.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=127.0.0.1,1433;Database=NTC;User Id=sa;Password=sa;TrustServerCertificate=True;"));

// Habilitar CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://192.168.18.2:3000") // Asegúrate de incluir la IP correcta
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Agregar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NFC API", Version = "v1" });
});

// Agregar soporte para controladores
builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();
// Aplicar CORS **ANTES** de cualquier middleware de seguridad
app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

// Configurar Swagger solo en desarrollo
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NFC API V1");
});

// Mapear controladores
app.MapControllers();

app.Run();
