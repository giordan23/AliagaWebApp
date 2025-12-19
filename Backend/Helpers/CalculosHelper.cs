namespace Backend.Helpers;

public static class CalculosHelper
{
    /// <summary>
    /// Calcula el saldo esperado de la caja
    /// Saldo = MontoInicial + TotalIngresos - TotalEgresos
    /// </summary>
    public static decimal CalcularSaldoEsperado(
        decimal montoInicial,
        decimal totalIngresos,
        decimal totalEgresos)
    {
        return Math.Round(montoInicial + totalIngresos - totalEgresos, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calcula la diferencia entre arqueo real y saldo esperado
    /// </summary>
    public static decimal CalcularDiferencia(decimal arqueoReal, decimal saldoEsperado)
    {
        return Math.Round(arqueoReal - saldoEsperado, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calcula el peso neto de una compra
    /// </summary>
    public static decimal CalcularPesoNeto(decimal pesoBruto, decimal descuentoKg)
    {
        return Math.Round(pesoBruto - descuentoKg, 1, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calcula el monto total de una transacción
    /// </summary>
    public static decimal CalcularMontoTotal(decimal peso, decimal precioPorKg)
    {
        return Math.Round(peso * precioPorKg, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calcula el saldo después de un movimiento de préstamo
    /// </summary>
    public static decimal CalcularSaldoPrestamoDespues(
        decimal saldoActual,
        decimal montoPrestamo,
        bool esPrestamo)
    {
        if (esPrestamo)
            return Math.Round(saldoActual + montoPrestamo, 2, MidpointRounding.AwayFromZero);
        else
            return Math.Round(saldoActual - montoPrestamo, 2, MidpointRounding.AwayFromZero);
    }
}
