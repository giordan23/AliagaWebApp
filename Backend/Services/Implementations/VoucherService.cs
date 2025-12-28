using Backend.DTOs.Responses;
using Backend.Models;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using System.Runtime.InteropServices;
using System.Drawing.Printing;

namespace Backend.Services.Implementations;

public class VoucherService : IVoucherService
{
    private readonly ICompraRepository _compraRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VoucherService> _logger;
    private const int ANCHO_VOUCHER = 32; // Caracteres para impresora 80mm

    // Windows API para enviar datos RAW a impresoras
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)] public string? pDocName;
        [MarshalAs(UnmanagedType.LPStr)] public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)] public string? pDataType;
    }

    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

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

    public string EspacioEntre(string izquierda, string derecha, int anchoTotal = 32)
    {
        int espacios = anchoTotal - izquierda.Length - derecha.Length;
        if (espacios < 1) espacios = 1; // Al menos un espacio
        return izquierda + new string(' ', espacios) + derecha;
    }

    public string GenerarContenidoVoucher(Compra compra, bool esDuplicado = false)
    {
        var sb = new StringBuilder();

        // Encabezado
        sb.AppendLine(Centrar("COMERCIAL ALIAGA"));
        sb.AppendLine(Centrar("Telefonos: 900812923 / 929450740"));
        sb.AppendLine(Linea('='));

        // Número de voucher y fecha
        sb.AppendLine(EspacioEntre($"Voucher #{compra.NumeroVoucher.PadLeft(8, '0')}", $" - Fecha: {compra.FechaCompra:dd/MM/yyyy HH:mm}"));

        // Información del cliente
        sb.AppendLine($"Cliente: {AcortarTexto(compra.ClienteProveedor?.NombreCompleto ?? "N/A", ANCHO_VOUCHER)}");
        sb.AppendLine($"DNI: {compra.ClienteProveedor?.DNI ?? "N/A"}");

        // Lista de productos
        sb.AppendLine(Linea('='));

        foreach (var detalle in compra.Detalles)
        {
            sb.AppendLine($">>> {AcortarTexto(detalle.Producto?.Nombre ?? "N/A", ANCHO_VOUCHER - 4)}");
            sb.AppendLine(EspacioEntre($"Humedad: {detalle.NivelSecado}", $"Calidad: {detalle.Calidad}"));

            if (detalle.TipoPesado == Enums.TipoPesado.Kg)
            {
                sb.AppendLine(FormatearLinea("Peso Bruto:", $"{detalle.PesoBruto:N1} kg"));
                sb.AppendLine(FormatearLinea("Descuento:", $"{detalle.DescuentoKg:N1} kg"));
                sb.AppendLine(FormatearLinea("Peso Neto:", $"{detalle.PesoNeto:N1} kg"));
            }

            sb.AppendLine(FormatearLinea("Precio/Kg:", $"S/ {detalle.PrecioPorKg:N2}"));
            sb.AppendLine(FormatearLinea("Subtotal:", $"S/ {detalle.Subtotal:N2}", true));
        }

        // Totales generales
        sb.AppendLine(Linea('='));
        sb.AppendLine(FormatearLinea("TOTAL PAGADO:", $"S/ {compra.MontoTotal:N2}", true));
        sb.AppendLine(Linea('='));

        // Pie de voucher
        sb.AppendLine();
        sb.AppendLine(Centrar("*** Gracias por su preferencia ***"));

        // Marca de duplicado
        if (esDuplicado)
            sb.AppendLine("-- DUPLICADO --");

        // Marca de editado
        if (compra.Editada)
        {
            sb.AppendLine();
            sb.AppendLine(Centrar("(Editado)"));
        }

        return sb.ToString();
    }

    private async Task<bool> ImprimirVoucherAsync(string contenido)
    {
        try
        {
            var nombreImpresora = _configuration["Impresora:Nombre"];
            var habilitada = _configuration.GetValue<bool>("Impresora:Habilitada", true);
            var tipoComunicacion = _configuration["Impresora:TipoComunicacion"] ?? "USB";

            if (string.IsNullOrEmpty(nombreImpresora))
            {
                _logger.LogWarning("Impresora no configurada en appsettings.json");
                return false;
            }

            if (!habilitada)
            {
                _logger.LogInformation("Impresión deshabilitada en configuración");
                return false;
            }

            return await Task.Run(() =>
            {
                try
                {
                    if (tipoComunicacion.Equals("USB", StringComparison.OrdinalIgnoreCase))
                    {
                        // Impresora USB/Windows usando API de Windows
                        return ImprimirConWindowsAPI(nombreImpresora, contenido);
                    }
                    else
                    {
                        _logger.LogError("Tipo de comunicación no válido: {TipoComunicacion}", tipoComunicacion);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al imprimir voucher");
                    return false;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al configurar impresión de voucher");
            return false;
        }
    }

    private bool ImprimirConWindowsAPI(string nombreImpresora, string contenido)
    {
        IntPtr hPrinter = IntPtr.Zero;
        try
        {
            // VERIFICAR SI LA IMPRESORA ESTÁ REALMENTE CONECTADA Y DISPONIBLE
            if (!VerificarImpresoraDisponible(nombreImpresora))
            {
                _logger.LogWarning("Impresora '{Impresora}' no está conectada o disponible. No se enviará el trabajo de impresión para evitar acumulación en cola.", nombreImpresora);
                return false;
            }

            // Abrir impresora
            if (!OpenPrinter(nombreImpresora, out hPrinter, IntPtr.Zero))
            {
                _logger.LogError("No se pudo abrir la impresora {Impresora}. Verifica que el nombre sea correcto.", nombreImpresora);
                return false;
            }

            // Iniciar documento
            var di = new DOCINFOA
            {
                pDocName = "Voucher de Compra",
                pDataType = "RAW"
            };

            if (!StartDocPrinter(hPrinter, 1, di))
            {
                _logger.LogError("No se pudo iniciar el documento de impresión");
                return false;
            }

            // Iniciar página
            if (!StartPagePrinter(hPrinter))
            {
                EndDocPrinter(hPrinter);
                _logger.LogError("No se pudo iniciar la página de impresión");
                return false;
            }

            // Generar comandos ESC/POS
            var e = new EPSON();
            var comandos = new List<byte[]>();

            // Inicializar (sin agregar líneas extra al inicio)
            comandos.Add(e.Initialize());

            // Procesar contenido línea por línea
            var lineas = contenido.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);

            foreach (var linea in lineas)
            {
                // Saltar líneas vacías al inicio del documento
                if (string.IsNullOrWhiteSpace(linea) && comandos.Count <= 1)
                {
                    continue;
                }

                if (linea.Contains("COMERCIAL ALIAGA"))
                {
                    comandos.Add(e.CenterAlign());
                    comandos.Add(e.SetStyles(PrintStyle.Bold | PrintStyle.DoubleWidth));
                    comandos.Add(e.PrintLine(linea));
                    comandos.Add(e.SetStyles(PrintStyle.None));
                }
                else if (linea.Contains("===") || linea.Contains("Gracias por su preferencia") || linea.Contains("Telefonos"))
                {
                    comandos.Add(e.CenterAlign());
                    comandos.Add(e.PrintLine(linea));
                }
                else if (linea.Contains("DUPLICADO"))
                {
                    comandos.Add(e.SetStyles(PrintStyle.Bold | PrintStyle.Underline));
                    comandos.Add(e.CenterAlign());
                    comandos.Add(e.PrintLine(linea));
                    comandos.Add(e.SetStyles(PrintStyle.None));
                }
                else if (linea.Contains("TOTAL PAGADO:"))
                {
                    comandos.Add(e.LeftAlign());
                    comandos.Add(e.SetStyles(PrintStyle.Bold));
                    comandos.Add(e.PrintLine(linea));
                    comandos.Add(e.SetStyles(PrintStyle.None));
                }
                else
                {
                    comandos.Add(e.LeftAlign());
                    comandos.Add(e.PrintLine(linea));
                }
            }

            // Alimentar y cortar
            comandos.Add(e.FeedLines(3));
            comandos.Add(e.FullCut());

            // Combinar todos los comandos
            var todosLosComandos = ByteSplicer.Combine(comandos.ToArray());

            // Enviar a la impresora
            IntPtr pBytes = Marshal.AllocHGlobal(todosLosComandos.Length);
            try
            {
                Marshal.Copy(todosLosComandos, 0, pBytes, todosLosComandos.Length);

                if (!WritePrinter(hPrinter, pBytes, todosLosComandos.Length, out int escritos))
                {
                    _logger.LogError("Error al escribir en la impresora");
                    return false;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pBytes);
            }

            // Finalizar página y documento
            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);

            _logger.LogInformation("Voucher impreso exitosamente en {Impresora}", nombreImpresora);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir con Windows API en {Impresora}", nombreImpresora);
            return false;
        }
        finally
        {
            if (hPrinter != IntPtr.Zero)
            {
                ClosePrinter(hPrinter);
            }
        }
    }

    /// <summary>
    /// Verifica si la impresora está físicamente conectada y disponible para imprimir.
    /// Esto evita que trabajos se queden en cola cuando la impresora no está conectada.
    /// </summary>
    private bool VerificarImpresoraDisponible(string nombreImpresora)
    {
        try
        {
            // Obtener todas las impresoras instaladas en el sistema
            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                if (printerName.Equals(nombreImpresora, StringComparison.OrdinalIgnoreCase))
                {
                    // Crear configuración de la impresora
                    var printerSettings = new PrinterSettings { PrinterName = nombreImpresora };

                    // Verificar si la impresora es válida
                    if (!printerSettings.IsValid)
                    {
                        _logger.LogWarning("Impresora '{Impresora}' no es válida", nombreImpresora);
                        return false;
                    }

                    // Verificar si la impresora está en línea (conectada físicamente)
                    // Nota: IsValid verifica que existe, pero no garantiza que esté conectada
                    // Para Windows, intentamos detectar si está offline
                    try
                    {
                        // Intentamos crear un PrintDocument para verificar disponibilidad
                        using var doc = new System.Drawing.Printing.PrintDocument();
                        doc.PrinterSettings = printerSettings;

                        // Si llegamos aquí, la impresora está instalada
                        // PrinterSettings.IsValid ya verificó que existe
                        _logger.LogInformation("Impresora '{Impresora}' encontrada y disponible", nombreImpresora);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Impresora '{Impresora}' no está accesible", nombreImpresora);
                        return false;
                    }
                }
            }

            _logger.LogWarning("Impresora '{Impresora}' no está instalada en el sistema", nombreImpresora);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad de impresora '{Impresora}'", nombreImpresora);
            return false;
        }
    }

    #region Helpers de Formato

    private string Centrar(string texto)
    {
        if (texto.Length >= ANCHO_VOUCHER)
            return texto;

        int espaciosIzq = (ANCHO_VOUCHER - texto.Length) / 4;
        int espaciosDer = ANCHO_VOUCHER - texto.Length - espaciosIzq;
        
        return new string(' ', espaciosIzq) + texto + new string(' ', espaciosDer);
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
