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

            // Obtener URL y Token de API RENIEC desde configuración
            var apiUrl = _configuration["ReniecApi:Url"];
            var apiToken = _configuration["ReniecApi:Token"];

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiToken))
            {
                _logger.LogWarning("API RENIEC no configurada");
                return new ReniecResponse
                {
                    Success = false,
                    Message = "API RENIEC no configurada. Puede registrar manualmente."
                };
            }

            _logger.LogInformation("Consultando DNI {DNI} en RENIEC", dni);

            // Crear la petición POST con el bearer token
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            requestMessage.Headers.Add("Authorization", $"Bearer {apiToken}");
            requestMessage.Headers.Add("Accept", "application/json");

            // Crear el body con el DNI en formato JSON
            var jsonBody = $"{{\"dni\":\"{dni}\"}}";
            requestMessage.Content = new System.Net.Http.StringContent(
                jsonBody,
                System.Text.Encoding.UTF8,
                "application/json"
            );

            // Realizar la llamada a la API de RENIEC
            var response = await _httpClient.SendAsync(requestMessage);

            // Leer el contenido de la respuesta para debugging
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("RENIEC Response Status: {StatusCode}, Body: {Body}", response.StatusCode, responseBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error en respuesta de API RENIEC. Status: {StatusCode}, Body: {Body}", response.StatusCode, responseBody);

                // Intentar parsear la respuesta de error por si contiene información útil
                try
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var errorResponse = System.Text.Json.JsonSerializer.Deserialize<ReniecApiResponse>(responseBody, options);
                    if (errorResponse != null && !errorResponse.Success)
                    {
                        // La API respondió con un error controlado (HTTP 500 pero con JSON válido)
                        _logger.LogInformation("RENIEC no encontró datos (error 500 con success=false): {Message}", errorResponse.Message);
                        return new ReniecResponse
                        {
                            Success = false,
                            Message = errorResponse.Message ?? "No se encontraron datos en RENIEC. Puede registrar manualmente."
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo parsear la respuesta de error de RENIEC");
                }

                return new ReniecResponse
                {
                    Success = false,
                    Message = "Error al consultar RENIEC. Puede registrar manualmente."
                };
            }

            // Parsear la respuesta (ya leímos el body antes para logging)
            ReniecApiResponse? jsonResponse = null;
            try
            {
                jsonResponse = System.Text.Json.JsonSerializer.Deserialize<ReniecApiResponse>(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deserializar respuesta de RENIEC");
            }

            if (jsonResponse == null)
            {
                _logger.LogWarning("Respuesta vacía de RENIEC para DNI {DNI}", dni);
                return new ReniecResponse
                {
                    Success = false,
                    Message = "Error al procesar respuesta de RENIEC. Puede registrar manualmente."
                };
            }

            // Si la API responde con success=false (no encontró datos)
            if (!jsonResponse.Success)
            {
                var mensajeApi = jsonResponse.Message ?? "No se encontraron datos";
                _logger.LogInformation("RENIEC no encontró datos para DNI {DNI}: {Message}", dni, mensajeApi);
                return new ReniecResponse
                {
                    Success = false,
                    Message = $"{mensajeApi}. Puede registrar manualmente."
                };
            }

            // Si success=true pero no hay data
            if (jsonResponse.Data == null)
            {
                _logger.LogWarning("RENIEC respondió success pero sin datos para DNI {DNI}", dni);
                return new ReniecResponse
                {
                    Success = false,
                    Message = "No se encontraron datos en RENIEC. Puede registrar manualmente."
                };
            }

            // Construir el nombre completo en el formato: NOMBRES APELLIDO_PATERNO APELLIDO_MATERNO
            var nombreCompleto = $"{jsonResponse.Data.Nombres} {jsonResponse.Data.ApellidoPaterno} {jsonResponse.Data.ApellidoMaterno}".Trim();

            return new ReniecResponse
            {
                Success = true,
                NombreCompleto = nombreCompleto,
                Message = "Datos obtenidos exitosamente de RENIEC"
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

    // Clases internas para deserializar la respuesta de la API externa
    private class ReniecApiResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public ReniecApiData? Data { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    private class ReniecApiData
    {
        [System.Text.Json.Serialization.JsonPropertyName("numero")]
        public string Numero { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("nombre_completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("apellido_paterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("apellido_materno")]
        public string ApellidoMaterno { get; set; } = string.Empty;
    }
}
