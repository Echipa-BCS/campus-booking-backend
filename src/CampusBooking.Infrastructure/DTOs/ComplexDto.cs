namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Data transfer object representing a sports complex for the catalog.
/// </summary>
public class ComplexDto
{
    /// <summary>Unique identifier of the complex.</summary>
    public int Id { get; set; }

    /// <summary>Name of the complex.</summary>
    public string Nume { get; set; } = string.Empty;

    /// <summary>Address of the complex.</summary>
    public string Adresa { get; set; } = string.Empty;

    /// <summary>Optional description of the complex.</summary>
    public string? Descriere { get; set; }

    /// <summary>Opening time of the complex.</summary>
    public TimeSpan OraDeschidere { get; set; }

    /// <summary>Closing time of the complex.</summary>
    public TimeSpan OraInchidere { get; set; }
}
