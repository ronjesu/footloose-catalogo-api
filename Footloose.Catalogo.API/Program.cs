using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Footloose.Catalogo.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Conectar Entity Framework Core con PostgreSQL (Supabase)
var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection");
builder.Services.AddDbContext<FootlooseDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Habilitar Controladores y OpenAPI / Scalar UI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 3. Habilitar CORS para futuras pruebas desde frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Habilitar interfaz interactiva de pruebas en /scalar/v1
app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
