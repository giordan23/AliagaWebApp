using Backend.DTOs.Responses;
using Backend.Services.Interfaces;

namespace Backend.Services.Implementations;

public class ReniecService : IReniecService
{
    private readonly ILogger<ReniecService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public ReniecService(ILogger<ReniecService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    public async Task<ReniecResponse> ConsultarDniAsync(string dni)
    {
        try
        {
            // Validar DNI
            if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8 || !dni.All(char.IsDigit))
            {
                return new ReniecResponse
                {
                    Success = false,
                    Message = "DNI inválido"
                };
            }

            // Obtener URL de API RENIEC desde configuración
            var apiUrl = _configuration["ReniecApi:Url"];
            var apiToken = _configuration["ReniecApi:Token"];

            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogWarning("API RENIEC no configurada");
                return new ReniecResponse
                {
                    Success = false,
                    Message = "API RENIEC no configurada. Puede registrar manualmente."
                };
            }

            // Aquí iría la implementación real de la consulta a RENIEC
            // Por ahora retornamos un fallback para permitir registro manual
            _logger.LogInformation("Consultando DNI {DNI} en RENIEC (simulado)", dni);

            // NOTA: Implementar la llamada real cuando se tenga acceso a la API
            // var response = await _httpClient.GetAsync($"{apiUrl}/consulta?dni={dni}");
            // if (response.IsSuccessStatusCode) { ... }

            // Fallback: retornar que no está disponible para permitir registro manual
            return new ReniecResponse
            {
                Success = false,
                Message = "API RENIEC no disponible. Puede registrar manualmente."
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al consultar RENIEC para DNI {DNI}", dni);
            return new ReniecResponse
            {
                Success = false,
                Message = "Error de conexión con RENIEC. Puede registrar manualmente."
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al consultar RENIEC para DNI {DNI}", dni);
            return new ReniecResponse
            {
                Success = false,
                Message = "Timeout en consulta RENIEC. Puede registrar manualmente."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar RENIEC para DNI {DNI}", dni);
            return new ReniecResponse
            {
                Success = false,
                Message = "Error al consultar RENIEC. Puede registrar manualmente."
            };
        }
    }
}
