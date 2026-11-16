using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Footloose.Catalogo.API.Data;

namespace Footloose.Catalogo.API.Controllers;

[ApiController]
[Route("api/v1/busqueda")]
public class BusquedaProductosController : ControllerBase
{
    private readonly FootlooseDbContext _context;

    public BusquedaProductosController(FootlooseDbContext context)
    {
        _context = context;
    }

    // SERVICIO 1: Consultar términos más buscados
    [HttpGet("terminos-mas-buscados")]
    public async Task<IActionResult> ConsultarTerminosMasBuscados()
    {
        var terminos = await _context.TerminosBusqueda
            .Where(t => t.PosicionRanking != null)
            .OrderBy(t => t.PosicionRanking)
            .Select(t => new
            {
                posicion = t.PosicionRanking,
                termino = t.Termino
            })
            .ToListAsync();

        return Ok(terminos);
    }

    // SERVICIO 2: Consultar sugerencias de búsqueda
    [HttpGet("sugerencias")]
    public async Task<IActionResult> ConsultarSugerencias([FromQuery] string? textoBusqueda)
    {
        if (string.IsNullOrWhiteSpace(textoBusqueda))
            return Ok(new List<string>());

        var filtro = textoBusqueda.Trim().ToLower();

        var sugerencias = await _context.TerminosBusqueda
            .Where(t => t.Termino.ToLower().Contains(filtro))
            .OrderByDescending(t => t.CantidadBusquedas)
            .Select(t => t.Termino)
            .Take(8)
            .ToListAsync();

        return Ok(sugerencias);
    }

    // SERVICIO 3: Consultar productos sugeridos
    [HttpGet("productos-sugeridos")]
    public async Task<IActionResult> ConsultarProductosSugeridos([FromQuery] string? textoBusqueda)
    {
        if (string.IsNullOrWhiteSpace(textoBusqueda))
            return Ok(new List<object>());

        var filtro = textoBusqueda.Trim().ToLower();

        var productos = await _context.Productos
            .Include(p => p.Marca)
            .Include(p => p.Variantes)
            .Where(p => p.Nombre.ToLower().Contains(filtro) ||
                        p.SkuBase.ToLower().Contains(filtro) ||
                        p.Marca.Nombre.ToLower().Contains(filtro))
            .Take(6)
            .Select(p => new
            {
                idProducto = p.IdProducto,
                sku = p.SkuBase,
                imagen = p.ImagenPrincipal,
                nombre = p.Nombre,
                marca = p.Marca.Nombre,
                precioNormal = p.PrecioNormal,
                precioMostrado = p.PrecioOferta ?? p.PrecioLlevateUnPar ?? p.PrecioNormal,
                colores = p.Variantes.Select(v => v.Color).Distinct().ToList()
            })
            .ToListAsync();

        return Ok(productos);
    }
}
