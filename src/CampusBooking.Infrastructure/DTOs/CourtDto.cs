namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Data transfer object representing a sports court (teren) for search results.
/// </summary>
public class CourtDto
{
    /// <summary>Unique identifier of the court.</summary>
    public int Id { get; set; }

    /// <summary>Identifier of the complex the court belongs to.</summary>
    public int ComplexId { get; set; }

    /// <summary>Name of the complex the court belongs to.</summary>
    public string ComplexNume { get; set; } = string.Empty;

    /// <summary>Name of the court.</summary>
    public string Nume { get; set; } = string.Empty;

    /// <summary>Type of sport played on the court.</summary>
    public string TipSport { get; set; } = string.Empty;

    /// <summary>Whether the court is covered (indoor).</summary>
    public bool EsteAcoperit { get; set; }

    /// <summary>Standard hourly rate of the court.</summary>
    public decimal TarifOrarStandard { get; set; }

    /// <summary>Minimum booking duration in hours.</summary>
    public int DurataMinimaOre { get; set; }
}
