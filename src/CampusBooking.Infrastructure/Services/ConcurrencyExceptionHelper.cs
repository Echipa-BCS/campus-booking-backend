using Microsoft.EntityFrameworkCore;

namespace CampusBooking.Infrastructure.Services;

/// <summary>
/// Shared helper for identifying database concurrency conflicts
/// (EF Core concurrency exceptions and MySQL InnoDB deadlocks / lock wait timeouts
/// raised under <see cref="System.Data.IsolationLevel.Serializable"/>).
/// </summary>
internal static class ConcurrencyExceptionHelper
{
    public static bool IsConcurrencyConflict(Exception ex)
    {
        var current = ex;
        while (current != null)
        {
            if (current is DbUpdateConcurrencyException)
            {
                return true;
            }

            var typeName = current.GetType().FullName ?? string.Empty;
            if (typeName.Contains("MySqlException", StringComparison.OrdinalIgnoreCase))
            {
                var message = current.Message ?? string.Empty;
                if (message.Contains("deadlock", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("lock wait timeout", StringComparison.OrdinalIgnoreCase)
                    || message.Contains("1213")
                    || message.Contains("1205"))
                {
                    return true;
                }
            }

            current = current.InnerException;
        }

        return false;
    }
}
