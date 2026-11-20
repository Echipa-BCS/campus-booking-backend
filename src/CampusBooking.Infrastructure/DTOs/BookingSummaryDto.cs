using System.Collections.Generic;

namespace CampusBooking.Infrastructure.DTOs;

/// <summary>
/// Summary of a booking for the user's booking history page.
/// </summary>
public class BookingSummaryDto
{
    public int Id { get; set; }
    public string CodRezervare { get; set; } = string.Empty;
    public int TerenId { get; set; }
    public string TerenNume { get; set; } = string.Empty;
    public string TipSport { get; set; } = string.Empty;
    public string ComplexNume { get; set; } = string.Empty;
    public DateTime DataStart { get; set; }
    public DateTime DataEnd { get; set; }
    public string Stare { get; set; } = string.Empty;
    public decimal PretTotal { get; set; }
    public decimal DiscountAplicat { get; set; }
    public string? DataExpirareBlocaj { get; set; }
    public List<RezervareEchipamentResponse> Echipamente { get; set; } = new();
}

/// <summary>
/// Represents an occupied or temporarily locked time slot on a court.
/// </summary>
public class OccupiedSlotDto
{
    public int Id { get; set; }
    public int TerenId { get; set; }
    public int UtilizatorId { get; set; }
    public DateTime DataStart { get; set; }
    public DateTime DataEnd { get; set; }
    public string Stare { get; set; } = string.Empty;
    public DateTime? DataExpirareBlocaj { get; set; }
}

/// <summary>
/// Equipment catalog item DTO.
/// </summary>
public class EchipamentDto
{
    public int Id { get; set; }
    public int ComplexId { get; set; }
    public string Denumire { get; set; } = string.Empty;
    public string TipSport { get; set; } = string.Empty;
    public int StocTotal { get; set; }
    public decimal TarifInchirierePerOra { get; set; }
}
