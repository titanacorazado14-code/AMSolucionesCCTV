namespace AMSoluciones.Domain.Comun;

/// <summary>
/// Fechas en UTC con precisión de segundos, igual que la columna datetimeoffset(0).
/// </summary>
internal static class Reloj
{
    public static DateTimeOffset AhoraUtc() => Truncar(DateTimeOffset.UtcNow);

    public static DateTimeOffset Truncar(DateTimeOffset fecha)
    {
        var ticks = fecha.UtcDateTime.Ticks;
        return new DateTimeOffset(ticks - ticks % TimeSpan.TicksPerSecond, TimeSpan.Zero);
    }
}
