using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Footloose.Catalogo.API.Data;

namespace Footloose.Catalogo.API.Controllers;

[ApiController]
[Route("api/v1/catalogo")]
public class CatalogoController : ControllerBase
{
    private readonly FootlooseDbContext _context;

    public CatalogoController(FootlooseDbContext context)
    {
        _context = context;
    }

    // SERVICIO 4: Consultar productos del catálogo (con filtros dinámicos y paginación)
    [HttpGet("productos")]
    public async Task<IActionResult> ConsultarProductosCatalogo(
        [FromQuery] string? textoBusqueda,
        [FromQuery] string? categoria,
        [FromQuery] string? marca,
        [FromQuery] string? talla,
        [FromQuery] string? color,
        [FromQuery] decimal? precioMinimo,
        [FromQuery] decimal? precioMaximo,
        [FromQuery] string? orden = "relevancia",
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanioPagina = 12)
    {
        var query = _context.Productos
            .Include(p => p.Marca)
            .Include(p => p.Variantes)
            .Include(p => p.ProductosCategorias)
                .ThenInclude(pc => pc.Categoria)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(textoBusqueda))
        {
            var txt = textoBusqueda.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(txt) || p.SkuBase.ToLower().Contains(txt));
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var cat = categoria.Trim().ToLower();
            query = query.Where(p => p.ProductosCategorias.Any(pc => pc.Categoria.Nombre.ToLower() == cat));
        }

        if (!string.IsNullOrWhiteSpace(marca))
        {
            var mrc = marca.Trim().ToLower();
            query = query.Where(p => p.Marca.Nombre.ToLower() == mrc);
        }

        if (!string.IsNullOrWhiteSpace(talla))
        {
            query = query.Where(p => p.Variantes.Any(v => v.Talla == talla && v.Stock > 0));
        }

        if (!string.IsNullOrWhiteSpace(color))
        {
            var col = color.Trim().ToLower();
            query = query.Where(p => p.Variantes.Any(v => v.Color.ToLower() == col));
        }

        if (precioMinimo.HasValue)
            query = query.Where(p => (p.PrecioOferta ?? p.PrecioNormal) >= precioMinimo.Value);

        if (precioMaximo.HasValue)
            query = query.Where(p => (p.PrecioOferta ?? p.PrecioNormal) <= precioMaximo.Value);

        query = orden?.ToLower() switch
        {
            "precio_asc" => query.OrderBy(p => p.PrecioOferta ?? p.PrecioNormal),
            "precio_desc" => query.OrderByDescending(p => p.PrecioOferta ?? p.PrecioNormal),
            "novedades" => query.OrderByDescending(p => p.FechaLanzamiento),
            _ => query.OrderBy(p => p.IdProducto)
        };

        var totalResultados = await query.CountAsync();

        var productos = await query
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .Select(p => new
            {
                idProducto = p.IdProducto,
                sku = p.SkuBase,
                nombre = p.Nombre,
                marca = p.Marca.Nombre,
                imagen = p.ImagenPrincipal,
                precioNormal = p.PrecioNormal,
                precioOferta = p.PrecioOferta,
                precioLlevateUnPar = p.PrecioLlevateUnPar,
                porcentajeDescuento = p.PorcentajeDescuento,
                etiquetaPromocion = p.EtiquetaPromocion,
                coloresDisponibles = p.Variantes.Select(v => new { v.Color, v.CodigoHexColor }).Distinct().ToList(),
                tallasDisponibles = p.Variantes.Where(v => v.Stock > 0).Select(v => v.Talla).Distinct().ToList()
            })
            .ToListAsync();

        return Ok(new
        {
            cantidadResultados = totalResultados,
            paginaActual = pagina,
            productos
        });
    }

    // SERVICIO 5: Consultar categorías y opciones del catálogo (Jerarquía recursiva)
    [HttpGet("categorias")]
    public async Task<IActionResult> ConsultarCategorias([FromQuery] string? seccion)
    {
        if (string.IsNullOrWhiteSpace(seccion))
        {
            var principales = await _context.Categorias
                .Where(c => c.CategoriaPadreId == null || c.EsDestacadaInicio)
                .Select(c => new
                {
                    idCategoria = c.IdCategoria,
                    nombre = c.Nombre,
                    enlace = c.Enlace,
                    imagen = c.ImagenUrl,
                    esDestacadaInicio = c.EsDestacadaInicio
                })
                .ToListAsync();

            return Ok(principales);
        }

        var sec = seccion.Trim().ToLower();
        var subopciones = await _context.Categorias
            .Where(c => c.SeccionMenu != null && c.SeccionMenu.ToLower() == sec && c.CategoriaPadreId != null)
            .Select(c => new
            {
                idCategoria = c.IdCategoria,
                agrupacion = c.Agrupacion,
                nombre = c.Nombre,
                enlace = c.Enlace,
                imagen = c.ImagenUrl
            })
            .ToListAsync();

        return Ok(subopciones);
    }

    // SERVICIO 6: Consultar marcas publicadas
    [HttpGet("marcas")]
    public async Task<IActionResult> ConsultarMarcas()
    {
        var marcas = await _context.Marcas
            .Where(m => m.Activa)
            .OrderBy(m => m.IdMarca)
            .Select(m => new
            {
                idMarca = m.IdMarca,
                nombre = m.Nombre,
                enlace = m.Enlace,
                imagen = m.ImagenUrl
            })
            .ToListAsync();

        return Ok(marcas);
    }

    // SERVICIO 7: Consultar productos del bloque promocional
    [HttpGet("bloques")]
    public async Task<IActionResult> ConsultarProductosBloque(
        [FromQuery] string? coleccion = "Nueva colección",
        [FromQuery] string? categoria = null,
        [FromQuery] string? orden = "relevancia")
    {
        var query = _context.Productos
            .Include(p => p.Variantes)
            .Include(p => p.ProductosCategorias)
                .ThenInclude(pc => pc.Categoria)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(coleccion))
        {
            var col = coleccion.Trim().ToLower();
            query = query.Where(p => p.ColeccionBloque != null && p.ColeccionBloque.ToLower() == col);
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var cat = categoria.Trim().ToLower();
            query = query.Where(p => p.ProductosCategorias.Any(pc => pc.Categoria.Nombre.ToLower() == cat));
        }

        query = orden?.ToLower() switch
        {
            "precio_asc" => query.OrderBy(p => p.PrecioOferta ?? p.PrecioNormal),
            "precio_desc" => query.OrderByDescending(p => p.PrecioOferta ?? p.PrecioNormal),
            _ => query.OrderBy(p => p.IdProducto)
        };

        var productosBloque = await query
            .Select(p => new
            {
                idProducto = p.IdProducto,
                imagen = p.ImagenPrincipal,
                nombre = p.Nombre,
                precioNormal = p.PrecioNormal,
                precioMostrado = p.PrecioOferta ?? p.PrecioLlevateUnPar ?? p.PrecioNormal,
                descuento = p.PorcentajeDescuento,
                etiqueta = p.EtiquetaPromocion,
                opcionesColor = p.Variantes.Select(v => new { v.Color, v.CodigoHexColor }).Distinct().ToList()
            })
            .ToListAsync();

        return Ok(productosBloque);
    }
}
