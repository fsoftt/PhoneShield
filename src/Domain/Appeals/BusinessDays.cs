namespace Tranqui.Domain.Appeals;

/// <summary>Business-day arithmetic for legal deadlines. Skips weekends; public holidays are not modeled.</summary>
public static class BusinessDays
{
    public static DateTimeOffset Add(DateTimeOffset start, int businessDays)
    {
        var date = start;
        var remaining = businessDays;
        while (remaining > 0)
        {
            date = date.AddDays(1);
            if (date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                remaining--;
            }
        }

        return date;
    }
}
