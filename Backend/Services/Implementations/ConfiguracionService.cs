using Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Implementations;

public class ConfiguracionService : IConfiguracionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfiguracionService> _logger;
    private const string DATABASE_FILENAME = "miapp.db";

    public ConfiguracionService(
        IConfiguration configuration,
        ILogger<ConfiguracionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]> GenerarBackupAsync()
    {
        try
        {
            // Obtener la ruta del archivo de base de datos
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), DATABASE_FILENAME);

            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException($"No se encontró el archivo de base de datos: {dbPath}");
            }

            // Leer el archivo completo
            var backupData = await File.ReadAllBytesAsync(dbPath);

            _logger.LogInformation("Backup generado exitosamente. Tamaño: {Size} bytes", backupData.Length);

            return backupData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar backup");
            throw;
        }
    }

    public string ObtenerNombreArchivoBackup()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"backup_aliaga_{timestamp}.db";
    }
}
