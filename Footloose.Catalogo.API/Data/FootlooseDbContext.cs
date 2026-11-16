using Microsoft.EntityFrameworkCore;
using Footloose.Catalogo.API.Models;

namespace Footloose.Catalogo.API.Data;

public class FootlooseDbContext : DbContext
{
    public FootlooseDbContext(DbContextOptions<FootlooseDbContext> options) : base(options) { }

    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<VarianteProducto> VariantesProductos { get; set; }
    public DbSet<ProductoCategoria> ProductosCategorias { get; set; }
    public DbSet<TerminoBusqueda> TerminosBusqueda { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar clave primaria compuesta para la tabla intermedia N:M
        modelBuilder.Entity<ProductoCategoria>()
            .HasKey(pc => new { pc.ProductoId, pc.CategoriaId });
    }
}
