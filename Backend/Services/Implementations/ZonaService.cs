using Backend.Data;
using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Implementations;

public class ZonaService : IZonaService
{
    private readonly IZonaRepository _zonaRepository;
    private readonly AppDbContext _context;

    public ZonaService(IZonaRepository zonaRepository, AppDbContext context)
    {
        _zonaRepository = zonaRepository;
        _context = context;
    }

    public async Task<ZonaResponse?> GetByIdAsync(int id)
    {
        var zona = await _zonaRepository.GetByIdAsync(id);
        if (zona == null)
            return null;

        return MapToResponse(zona);
    }

    public async Task<List<ZonaResponse>> GetAllAsync(int skip = 0, int take = 50)
    {
        var zonas = await _zonaRepository.GetAllAsync(skip, take);
        return zonas.Select(z => MapToResponse(z)).ToList();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _zonaRepository.GetTotalCountAsync();
    }

    public async Task<ZonaResponse> CreateAsync(CrearZonaRequest request)
    {
        // Validar que no exista zona con ese nombre
        if (await _zonaRepository.ExisteNombreAsync(request.Nombre))
        {
            throw new InvalidOperationException("Ya existe una zona con ese nombre");
        }

        var zona = new Zona
        {
            Nombre = request.Nombre,
            FechaCreacion = DateTime.Now,
            FechaModificacion = DateTime.Now
        };

        var zonaCreada = await _zonaRepository.AddAsync(zona);
        return MapToResponse(zonaCreada);
    }

    public async Task<ZonaResponse> UpdateAsync(int id, ActualizarZonaRequest request)
    {
        var zona = await _zonaRepository.GetByIdAsync(id);
        if (zona == null)
        {
            throw new InvalidOperationException("Zona no encontrada");
        }

        // Validar que no exista otra zona con ese nombre
        if (await _zonaRepository.ExisteNombreAsync(request.Nombre, id))
        {
            throw new InvalidOperationException("Ya existe otra zona con ese nombre");
        }

        zona.Nombre = request.Nombre;
        await _zonaRepository.UpdateAsync(zona);

        return MapToResponse(zona);
    }

    public async Task<List<ClienteProveedorResponse>> GetClientesByZonaAsync(int zonaId)
    {
        var clientes = await _context.ClientesProveedores
            .Include(c => c.Zona)
            .Where(c => c.ZonaId == zonaId)
            .OrderBy(c => c.NombreCompleto)
            .ToListAsync();

        return clientes.Select(c => new ClienteProveedorResponse
        {
            Id = c.Id,
            DNI = c.DNI,
            NombreCompleto = c.NombreCompleto,
            Telefono = c.Telefono,
            Direccion = c.Direccion,
            FechaNacimiento = c.FechaNacimiento,
            ZonaId = c.ZonaId,
            ZonaNombre = c.Zona?.Nombre,
            SaldoPrestamo = c.SaldoPrestamo,
            EsAnonimo = c.EsAnonimo,
            FechaCreacion = c.FechaCreacion
        }).ToList();
    }

    private ZonaResponse MapToResponse(Zona zona)
    {
        return new ZonaResponse
        {
            Id = zona.Id,
            Nombre = zona.Nombre,
            CantidadClientes = zona.Clientes?.Count ?? 0,
            FechaCreacion = zona.FechaCreacion,
            FechaModificacion = zona.FechaModificacion
        };
    }
}
