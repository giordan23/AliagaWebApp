namespace Backend.Services.Interfaces;

public interface IConfiguracionService
{
    Task<byte[]> GenerarBackupAsync();
    string ObtenerNombreArchivoBackup();
}
