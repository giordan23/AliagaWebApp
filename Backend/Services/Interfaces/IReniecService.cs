using Backend.DTOs.Responses;

namespace Backend.Services.Interfaces;

public interface IReniecService
{
    Task<ReniecResponse> ConsultarDniAsync(string dni);
}
