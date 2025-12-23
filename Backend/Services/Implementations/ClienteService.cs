using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;

namespace Backend.Services.Implementations;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    // Clientes Proveedores
    public async Task<ClienteProveedorResponse?> GetProveedorByIdAsync(int id)
    {
        var cliente = await _clienteRepository.GetProveedorByIdAsync(id);
        if (cliente == null)
            return null;

        return MapProveedorToResponse(cliente);
    }

    public async Task<ClienteProveedorResponse?> GetProveedorByDniAsync(string dni)
    {
        var cliente = await _clienteRepository.GetProveedorByDniAsync(dni);
        if (cliente == null)
            return null;

        return MapProveedorToResponse(cliente);
    }

    public async Task<List<ClienteProveedorResponse>> GetProveedoresAsync(int skip = 0, int take = 50, string? searchTerm = null, int? zonaId = null)
    {
        var clientes = await _clienteRepository.GetProveedoresAsync(skip, take, searchTerm, zonaId);
        var kgVendidosPorProveedor = await _clienteRepository.GetTotalKgVendidosPorProveedorAsync();

        return clientes.Select(c => MapProveedorToResponse(c, kgVendidosPorProveedor)).ToList();
    }

    public async Task<int> GetTotalProveedoresCountAsync(string? searchTerm = null, int? zonaId = null)
    {
        return await _clienteRepository.GetTotalProveedoresCountAsync(searchTerm, zonaId);
    }

    public async Task<ClienteProveedorResponse> CreateProveedorAsync(CrearClienteProveedorRequest request)
    {
        // Validar DNI único
        if (await _clienteRepository.ExisteDniAsync(request.DNI))
        {
            throw new InvalidOperationException("Ya existe un cliente con ese DNI");
        }

        // No permitir crear cliente anónimo manualmente
        if (request.DNI == "00000000")
        {
            throw new InvalidOperationException("El cliente anónimo ya existe en el sistema");
        }

        var cliente = new ClienteProveedor
        {
            DNI = request.DNI,
            NombreCompleto = request.NombreCompleto,
            Telefono = request.Telefono,
            Direccion = request.Direccion,
            FechaNacimiento = request.FechaNacimiento,
            ZonaId = request.ZonaId,
            SaldoPrestamo = 0,
            EsAnonimo = false,
            FechaCreacion = DateTime.Now,
            FechaModificacion = DateTime.Now
        };

        var clienteCreado = await _clienteRepository.AddProveedorAsync(cliente);

        // Recargar con Zona incluida
        return MapProveedorToResponse(await _clienteRepository.GetProveedorByIdAsync(clienteCreado.Id) ?? clienteCreado);
    }

    public async Task<ClienteProveedorResponse> UpdateProveedorAsync(int id, ActualizarClienteProveedorRequest request)
    {
        var cliente = await _clienteRepository.GetProveedorByIdAsync(id);
        if (cliente == null)
        {
            throw new InvalidOperationException("Cliente no encontrado");
        }

        // No permitir editar cliente anónimo
        if (cliente.EsAnonimo)
        {
            throw new InvalidOperationException("El cliente anónimo no se puede editar");
        }

        cliente.NombreCompleto = request.NombreCompleto;
        cliente.Telefono = request.Telefono;
        cliente.Direccion = request.Direccion;
        cliente.FechaNacimiento = request.FechaNacimiento;
        cliente.ZonaId = request.ZonaId;

        await _clienteRepository.UpdateProveedorAsync(cliente);

        // Recargar con Zona incluida
        return MapProveedorToResponse(await _clienteRepository.GetProveedorByIdAsync(id) ?? cliente);
    }

    public async Task DeleteProveedorAsync(int id)
    {
        var cliente = await _clienteRepository.GetProveedorByIdAsync(id);
        if (cliente == null)
        {
            throw new InvalidOperationException("Cliente no encontrado");
        }

        // No permitir eliminar cliente anónimo
        if (cliente.EsAnonimo)
        {
            throw new InvalidOperationException("El cliente anónimo no se puede eliminar");
        }

        await _clienteRepository.DeleteProveedorAsync(id);
    }

    // Clientes Compradores
    public async Task<ClienteCompradorResponse?> GetCompradorByIdAsync(int id)
    {
        var cliente = await _clienteRepository.GetCompradorByIdAsync(id);
        if (cliente == null)
            return null;

        return MapCompradorToResponse(cliente);
    }

    public async Task<List<ClienteCompradorResponse>> GetCompradoresAsync()
    {
        var clientes = await _clienteRepository.GetCompradoresAsync();
        return clientes.Select(c => MapCompradorToResponse(c)).ToList();
    }

    public async Task<ClienteCompradorResponse> CreateCompradorAsync(CrearClienteCompradorRequest request)
    {
        var cliente = new ClienteComprador
        {
            Nombre = request.Nombre,
            FechaCreacion = DateTime.Now,
            FechaModificacion = DateTime.Now
        };

        var clienteCreado = await _clienteRepository.AddCompradorAsync(cliente);
        return MapCompradorToResponse(clienteCreado);
    }

    public async Task<ClienteCompradorResponse> UpdateCompradorAsync(int id, CrearClienteCompradorRequest request)
    {
        var cliente = await _clienteRepository.GetCompradorByIdAsync(id);
        if (cliente == null)
        {
            throw new InvalidOperationException("Cliente comprador no encontrado");
        }

        cliente.Nombre = request.Nombre;
        await _clienteRepository.UpdateCompradorAsync(cliente);

        return MapCompradorToResponse(cliente);
    }

    public async Task DeleteCompradorAsync(int id)
    {
        var cliente = await _clienteRepository.GetCompradorByIdAsync(id);
        if (cliente == null)
        {
            throw new InvalidOperationException("Cliente comprador no encontrado");
        }

        await _clienteRepository.DeleteCompradorAsync(id);
    }

    // Mappers
    private ClienteProveedorResponse MapProveedorToResponse(ClienteProveedor cliente, Dictionary<int, decimal>? kgVendidosPorProveedor = null)
    {
        decimal totalKgVendidos = 0;
        if (kgVendidosPorProveedor != null && kgVendidosPorProveedor.ContainsKey(cliente.Id))
        {
            totalKgVendidos = kgVendidosPorProveedor[cliente.Id];
        }

        return new ClienteProveedorResponse
        {
            Id = cliente.Id,
            DNI = cliente.DNI,
            NombreCompleto = cliente.NombreCompleto,
            Telefono = cliente.Telefono,
            Direccion = cliente.Direccion,
            FechaNacimiento = cliente.FechaNacimiento,
            ZonaId = cliente.ZonaId,
            ZonaNombre = cliente.Zona?.Nombre,
            SaldoPrestamo = cliente.SaldoPrestamo,
            TotalKgVendidos = totalKgVendidos,
            EsAnonimo = cliente.EsAnonimo,
            FechaCreacion = cliente.FechaCreacion
        };
    }

    private ClienteCompradorResponse MapCompradorToResponse(ClienteComprador cliente)
    {
        return new ClienteCompradorResponse
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            FechaCreacion = cliente.FechaCreacion
        };
    }
}
