using System.Globalization;

namespace StorePro.Web.Services;

public static class Format
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static string Money(decimal value) => value.ToString("$#,##0.00", Culture);

    public static string Number(int value) => value.ToString("N0", Culture);

    public static string Percent(decimal value) => value.ToString("0.#", Culture) + "%";

    public static string Date(DateTime? date) => date?.ToString("dd/MM/yyyy", Culture) ?? "—";

    public static string DateTimeFull(DateTime? date) => date?.ToString("dd/MM/yyyy HH:mm", Culture) ?? "—";

    /// <summary>Tiempo relativo en español: "hace 2 min", "hace 1 h", "12/10/2023".</summary>
    public static string Relative(DateTime? date)
    {
        if (date is null) return "—";
        var span = DateTime.Now - date.Value;
        if (span.TotalMinutes < 1) return "hace unos segundos";
        if (span.TotalMinutes < 60) return $"hace {(int)span.TotalMinutes} min";
        if (span.TotalHours < 24) return $"hace {(int)span.TotalHours} h";
        if (span.TotalDays < 30) return $"hace {(int)span.TotalDays} d";
        return date.Value.ToString("dd/MM/yyyy", Culture);
    }

    public static string Initials(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        var initials = parts[0][0].ToString();
        if (parts.Length > 1) initials += parts[^1][0];
        return initials.ToUpper(Culture);
    }
}
