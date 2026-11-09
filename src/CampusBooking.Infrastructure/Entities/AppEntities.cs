namespace CampusBooking.Infrastructure.Entities;

public enum RolUtilizator
{
    USER,
    STUDENT,
    PROFESOR,
    ADMIN
}

public enum StareRezervare
{
    TEMPORAR_BLOCATA,
    CONFIRMATA,
    EXPIRATA,
    ANULATA,
    FINALIZATA
}

public enum TipAbonament
{
    GENERAL_ACCESS,
    INTERVAL_FIX
}

public class ComplexeSportive
{
    public int Id { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Adresa { get; set; } = string.Empty;
    public TimeSpan OraDeschidere { get; set; }
    public TimeSpan OraInchidere { get; set; }
    public string? Descriere { get; set; }

    public ICollection<Teren> Terenuri { get; set; } = new List<Teren>();
    public ICollection<Echipament> Echipamente { get; set; } = new List<Echipament>();
}

public class Teren
{
    public int Id { get; set; }
    public int ComplexId { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string TipSport { get; set; } = string.Empty;
    public bool EsteAcoperit { get; set; }
    public decimal TarifOrarStandard { get; set; }
    public int DurataMinimaOre { get; set; }

    public ComplexeSportive Complex { get; set; } = null!;
    public ICollection<Rezervare> Rezervari { get; set; } = new List<Rezervare>();
}

public class Utilizator
{
    public int Id { get; set; }
    public string Nume { get; set; } = string.Empty;
    public string Prenume { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ParolaHash { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public RolUtilizator Rol { get; set; } = RolUtilizator.USER;
    public DateTime DataInregistrare { get; set; }

    public ICollection<Rezervare> Rezervari { get; set; } = new List<Rezervare>();
    public ICollection<Abonament> Abonamente { get; set; } = new List<Abonament>();
    public ICollection<Plata> Plati { get; set; } = new List<Plata>();
}

public class Echipament
{
    public int Id { get; set; }
    public int ComplexId { get; set; }
    public string Denumire { get; set; } = string.Empty;
    public string TipSport { get; set; } = string.Empty;
    public int StocTotal { get; set; }
    public decimal TarifInchirierePerOra { get; set; }

    public ComplexeSportive Complex { get; set; } = null!;
    public ICollection<RezervareEchipament> RezervareEchipamente { get; set; } = new List<RezervareEchipament>();
}

public class Rezervare
{
    public int Id { get; set; }
    public int UtilizatorId { get; set; }
    public int TerenId { get; set; }
    public string CodRezervare { get; set; } = string.Empty;
    public DateTime DataStart { get; set; }
    public DateTime DataEnd { get; set; }
    public DateTime? DataExpirareBlocaj { get; set; }
    public decimal PretTotal { get; set; }
    public decimal DiscountAplicat { get; set; }
    public StareRezervare Stare { get; set; } = StareRezervare.TEMPORAR_BLOCATA;

    public Utilizator Utilizator { get; set; } = null!;
    public Teren Teren { get; set; } = null!;
    public ICollection<RezervareEchipament> RezervareEchipamente { get; set; } = new List<RezervareEchipament>();
    public ICollection<Plata> Plati { get; set; } = new List<Plata>();
}

public class RezervareEchipament
{
    public int RezervareId { get; set; }
    public int EchipamentId { get; set; }
    public int Cantitate { get; set; }
    public decimal SubtotalPret { get; set; }

    public Rezervare Rezervare { get; set; } = null!;
    public Echipament Echipament { get; set; } = null!;
}

public class Abonament
{
    public int Id { get; set; }
    public int UtilizatorId { get; set; }
    public TipAbonament Tip { get; set; }
    public DateTime DataStart { get; set; }
    public DateTime DataExpirare { get; set; }
    public int SedinteRamase { get; set; }
    public bool Activ { get; set; }

    public Utilizator Utilizator { get; set; } = null!;
    public ICollection<Plata> Plati { get; set; } = new List<Plata>();
}

public class Plata
{
    public int Id { get; set; }
    public int UtilizatorId { get; set; }
    public int? RezervareId { get; set; }
    public int? AbonamentId { get; set; }
    public string StripeSessionId { get; set; } = string.Empty;
    public decimal Suma { get; set; }
    public DateTime DataPlata { get; set; }
    public string Stare { get; set; } = string.Empty;
    public string MetodaPlata { get; set; } = string.Empty;

    public Utilizator Utilizator { get; set; } = null!;
    public Rezervare? Rezervare { get; set; }
    public Abonament? Abonament { get; set; }
}
