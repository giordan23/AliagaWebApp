using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Backend.Services.Implementations;

public class VoucherService : IVoucherService
{
    private readonly ICompraRepository _compraRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VoucherService> _logger;
    private const int ANCHO_VOUCHER = 32; // Caracteres para impresora 80mm

    public VoucherService(
        ICompraRepository compraRepository,
        IConfiguration configuration,
        ILogger<VoucherService> logger)
    {
        _compraRepository = compraRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<VoucherResponse> GenerarYImprimirVoucherAsync(Compra compra, bool esDuplicado = false)
    {
        try
        {
            var contenido = GenerarContenidoVoucher(compra, esDuplicado);

            // Intentar imprimir (por ahora solo generamos el contenido)
            var impresionExitosa = await ImprimirVoucherAsync(contenido);

            return new VoucherResponse
            {
                NumeroVoucher = compra.NumeroVoucher,
                ContenidoVoucher = contenido,
                ImpresionExitosa = impresionExitosa,
                MensajeError = impresionExitosa ? null : "Impresora no configurada. Voucher generado pero no impreso."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar voucher para compra {CompraId}", compra.Id);
            return new VoucherResponse
            {
                NumeroVoucher = compra.NumeroVoucher,
                ContenidoVoucher = string.Empty,
                ImpresionExitosa = false,
                MensajeError = $"Error al generar voucher: {ex.Message}"
            };
        }
    }

    public async Task<VoucherResponse> ReimprimirVoucherAsync(int compraId)
    {
        var compra = await _compraRepository.GetByIdAsync(compraId);

        if (compra == null)
        {
            return new VoucherResponse
            {
                NumeroVoucher = string.Empty,
                ContenidoVoucher = string.Empty,
                ImpresionExitosa = false,
                MensajeError = "Compra no encontrada"
            };
        }

        // Reimprimir siempre marca como duplicado
        return await GenerarYImprimirVoucherAsync(compra, esDuplicado: true);
    }

    public string GenerarContenidoVoucher(Compra compra, bool esDuplicado = false)
    {
        var sb = new StringBuilder();

        // Encabezado
        sb.AppendLine(Centrar("================================"));
        sb.AppendLine(Centrar("SISTEMA COMERCIAL ALIAGA"));
        sb.AppendLine(Centrar("================================"));
        sb.AppendLine();

        // Número de voucher y fecha
        sb.AppendLine($"Voucher N°: {compra.NumeroVoucher.PadLeft(8, '0')}");
        sb.AppendLine($"Fecha: {compra.FechaCompra:dd/MM/yyyy HH:mm}");
        sb.AppendLine();

        // Información del cliente
        sb.AppendLine("CLIENTE");
        sb.AppendLine(Linea('-'));
        sb.AppendLine($"DNI: {compra.ClienteProveedor?.DNI ?? "N/A"}");
        sb.AppendLine($"Nombre: {AcortarTexto(compra.ClienteProveedor?.NombreCompleto ?? "N/A", ANCHO_VOUCHER)}");
        if (compra.ClienteProveedor?.Zona != null)
        {
            sb.AppendLine($"Zona: {AcortarTexto(compra.ClienteProveedor.Zona.Nombre, ANCHO_VOUCHER - 6)}");
        }
        sb.AppendLine();

        // Lista de productos
        sb.AppendLine($"PRODUCTOS ({compra.Detalles.Count})");
        sb.AppendLine(Linea('='));

        foreach (var detalle in compra.Detalles)
        {
            sb.AppendLine();
            sb.AppendLine($">>> {AcortarTexto(detalle.Producto?.Nombre ?? "N/A", ANCHO_VOUCHER - 4)}");
            sb.AppendLine(Linea('-'));
            sb.AppendLine($"Nivel Secado: {AcortarTexto(detalle.NivelSecado, ANCHO_VOUCHER - 14)}");
            sb.AppendLine($"Calidad: {detalle.Calidad}");
            sb.AppendLine($"Tipo Pesado: {detalle.TipoPesado}");

            if (detalle.TipoPesado == Enums.TipoPesado.Kg)
            {
                sb.AppendLine(FormatearLinea("Peso Bruto:", $"{detalle.PesoBruto:N1} kg"));
                sb.AppendLine(FormatearLinea("Descuento:", $"{detalle.DescuentoKg:N1} kg"));
                sb.AppendLine(FormatearLinea("Peso Neto:", $"{detalle.PesoNeto:N1} kg"));
            }
            else
            {
                sb.AppendLine(FormatearLinea("Valdeos:", $"{detalle.PesoBruto:N0}"));
                sb.AppendLine(FormatearLinea("Peso Neto:", $"{detalle.PesoNeto:N1} kg"));
            }

            sb.AppendLine(FormatearLinea("Precio/Kg:", $"S/ {detalle.PrecioPorKg:N2}"));
            sb.AppendLine(FormatearLinea("Subtotal:", $"S/ {detalle.Subtotal:N2}", true));
        }

        // Totales generales
        sb.AppendLine();
        sb.AppendLine(Linea('='));
        sb.AppendLine(FormatearLinea("PESO TOTAL:", $"{compra.PesoTotal:N1} kg", true));
        sb.AppendLine(Centrar("=========================="));
        sb.AppendLine(FormatearLinea("TOTAL A PAGAR:", $"S/ {compra.MontoTotal:N2}", true));
        sb.AppendLine(Centrar("=========================="));
        sb.AppendLine();

        // Pie de voucher
        sb.AppendLine(Centrar("Gracias por su preferencia"));
        sb.AppendLine();

        // Marca de duplicado
        if (esDuplicado)
        {
            sb.AppendLine();
            sb.AppendLine(Centrar("*** DUPLICADO ***"));
        }

        // Marca de editado
        if (compra.Editada)
        {
            sb.AppendLine();
            sb.AppendLine(Centrar("(Editado)"));
        }

        sb.AppendLine();
        sb.AppendLine(Centrar("================================"));

        return sb.ToString();
    }

    private async Task<bool> ImprimirVoucherAsync(string contenido)
    {
        try
        {
            var nombreImpresora = _configuration["Impresora:Nombre"];

            if (string.IsNullOrEmpty(nombreImpresora))
            {
                _logger.LogWarning("Impresora no configurada en appsettings.json");
                return false;
            }

            // TODO: Implementar integración con ESCPOS-NET para impresión térmica
            // Por ahora retornamos false para indicar que no se imprimió físicamente
            // pero el voucher fue generado exitosamente
            _logger.LogInformation("Voucher generado (impresión pendiente de implementación con ESCPOS-NET)");

            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir voucher");
            return false;
        }
    }

    #region Helpers de Formato

    private string Centrar(string texto)
    {
        if (texto.Length >= ANCHO_VOUCHER)
            return texto;

        int espacios = (ANCHO_VOUCHER - texto.Length) / 2;
        return new string(' ', espacios) + texto;
    }

    private string Linea(char caracter)
    {
        return new string(caracter, ANCHO_VOUCHER);
    }

    private string FormatearLinea(string etiqueta, string valor, bool resaltar = false)
    {
        int espaciosDisponibles = ANCHO_VOUCHER - etiqueta.Length - valor.Length;

        if (espaciosDisponibles < 1)
        {
            // Si no cabe, acortar etiqueta
            etiqueta = etiqueta.Substring(0, Math.Max(0, ANCHO_VOUCHER - valor.Length - 1));
            espaciosDisponibles = 1;
        }

        string linea = etiqueta + new string(' ', espaciosDisponibles) + valor;

        if (resaltar && linea.Length <= ANCHO_VOUCHER)
        {
            return linea;
        }

        return linea;
    }

    private string AcortarTexto(string texto, int longitudMaxima)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        if (texto.Length <= longitudMaxima)
            return texto;

        return texto.Substring(0, longitudMaxima - 3) + "...";
    }

    #endregion
}
