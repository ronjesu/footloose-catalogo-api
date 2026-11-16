using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Footloose.Catalogo.API.Models;

[Table("marcas")]
public class Marca
{
    [Key]
    [Column("id_marca")]
    public int IdMarca { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("enlace")]
    public string Enlace { get; set; } = string.Empty;

    [Column("imagen_url")]
    public string? ImagenUrl { get; set; }

    [Column("activa")]
    public bool Activa { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

[Table("categorias")]
public class Categoria
{
    [Key]
    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [Column("categoria_padre_id")]
    public int? CategoriaPadreId { get; set; }

    [ForeignKey("CategoriaPadreId")]
    public Categoria? CategoriaPadre { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("seccion_menu")]
    public string? SeccionMenu { get; set; }

    [Column("agrupacion")]
    public string? Agrupacion { get; set; }

    [Column("enlace")]
    public string Enlace { get; set; } = string.Empty;

    [Column("imagen_url")]
    public string? ImagenUrl { get; set; }

    [Column("es_destacada_inicio")]
    public bool EsDestacadaInicio { get; set; }

    public ICollection<Categoria> SubCategorias { get; set; } = new List<Categoria>();
    public ICollection<ProductoCategoria> ProductosCategorias { get; set; } = new List<ProductoCategoria>();
}

[Table("productos")]
public class Producto
{
    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("marca_id")]
    public int MarcaId { get; set; }

    [ForeignKey("MarcaId")]
    public Marca Marca { get; set; } = null!;

    [Column("sku_base")]
    public string SkuBase { get; set; } = string.Empty;

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("precio_normal")]
    public decimal PrecioNormal { get; set; }

    [Column("precio_oferta")]
    public decimal? PrecioOferta { get; set; }

    [Column("precio_llevate_un_par")]
    public decimal? PrecioLlevateUnPar { get; set; }

    [Column("porcentaje_descuento")]
    public int? PorcentajeDescuento { get; set; }

    [Column("etiqueta_promocion")]
    public string? EtiquetaPromocion { get; set; }

    [Column("coleccion_bloque")]
    public string? ColeccionBloque { get; set; }

    [Column("imagen_principal")]
    public string ImagenPrincipal { get; set; } = string.Empty;

    [Column("fecha_lanzamiento")]
    public DateTime FechaLanzamiento { get; set; }

    public ICollection<VarianteProducto> Variantes { get; set; } = new List<VarianteProducto>();
    public ICollection<ProductoCategoria> ProductosCategorias { get; set; } = new List<ProductoCategoria>();
}

[Table("variantes_productos")]
public class VarianteProducto
{
    [Key]
    [Column("id_variante")]
    public int IdVariante { get; set; }

    [Column("producto_id")]
    public int ProductoId { get; set; }

    [ForeignKey("ProductoId")]
    public Producto Producto { get; set; } = null!;

    [Column("sku_variante")]
    public string SkuVariante { get; set; } = string.Empty;

    [Column("color")]
    public string Color { get; set; } = string.Empty;

    [Column("codigo_hex_color")]
    public string? CodigoHexColor { get; set; }

    [Column("talla")]
    public string Talla { get; set; } = string.Empty;

    [Column("stock")]
    public int Stock { get; set; }

    [Column("imagen_color")]
    public string? ImagenColor { get; set; }
}

[Table("productos_categorias")]
public class ProductoCategoria
{
    [Column("producto_id")]
    public int ProductoId { get; set; }

    [ForeignKey("ProductoId")]
    public Producto Producto { get; set; } = null!;

    [Column("categoria_id")]
    public int CategoriaId { get; set; }

    [ForeignKey("CategoriaId")]
    public Categoria Categoria { get; set; } = null!;
}

[Table("terminos_busqueda")]
public class TerminoBusqueda
{
    [Key]
    [Column("id_termino")]
    public int IdTermino { get; set; }

    [Column("termino")]
    public string Termino { get; set; } = string.Empty;

    [Column("posicion_ranking")]
    public int? PosicionRanking { get; set; }

    [Column("cantidad_busquedas")]
    public int CantidadBusquedas { get; set; }
}
