using CampusBooking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CampusBooking.Infrastructure.Configurations;

public class ComplexeSportiveConfiguration : IEntityTypeConfiguration<ComplexeSportive>
{
    public void Configure(EntityTypeBuilder<ComplexeSportive> b)
    {
        b.ToTable("complexe_sportive");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.Nume).HasColumnName("nume").IsRequired().HasMaxLength(200);
        b.Property(x => x.Adresa).HasColumnName("adresa").IsRequired().HasMaxLength(500);
        b.Property(x => x.OraDeschidere).HasColumnName("ora_deschidere").HasColumnType("time").IsRequired();
        b.Property(x => x.OraInchidere).HasColumnName("ora_inchidere").HasColumnType("time").IsRequired();
        b.Property(x => x.Descriere).HasColumnName("descriere").HasColumnType("text");

        b.HasMany(x => x.Terenuri).WithOne(x => x.Complex).HasForeignKey(x => x.ComplexId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TerenConfiguration : IEntityTypeConfiguration<Teren>
{
    public void Configure(EntityTypeBuilder<Teren> b)
    {
        b.ToTable("terenuri");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.ComplexId).HasColumnName("complex_id").IsRequired();
        b.Property(x => x.Nume).HasColumnName("nume").IsRequired().HasMaxLength(200);
        b.Property(x => x.TipSport).HasColumnName("tip_sport").IsRequired().HasMaxLength(100);
        b.Property(x => x.EsteAcoperit).HasColumnName("este_acoperit").IsRequired();
        b.Property(x => x.TarifOrarStandard).HasColumnName("tarif_orar_standard").IsRequired().HasColumnType("decimal(10,2)");
        b.Property(x => x.DurataMinimaOre).HasColumnName("durata_minima_ore").IsRequired();

        b.HasOne(x => x.Complex).WithMany(x => x.Terenuri).HasForeignKey(x => x.ComplexId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class UtilizatorConfiguration : IEntityTypeConfiguration<Utilizator>
{
    public void Configure(EntityTypeBuilder<Utilizator> b)
    {
        b.ToTable("utilizatori");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.Nume).HasColumnName("nume").IsRequired().HasMaxLength(100);
        b.Property(x => x.Prenume).HasColumnName("prenume").IsRequired().HasMaxLength(100);
        b.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(200);
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.ParolaHash).HasColumnName("parola_hash").IsRequired().HasMaxLength(500);
        b.Property(x => x.Telefon).HasColumnName("telefon").HasMaxLength(50);
        b.Property(x => x.Rol).HasColumnName("rol").IsRequired().HasConversion<string>();
        b.Property(x => x.DataInregistrare).HasColumnName("data_inregistrare").IsRequired();

        b.HasMany(x => x.Rezervari).WithOne(x => x.Utilizator).HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Abonamente).WithOne(x => x.Utilizator).HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Plati).WithOne(x => x.Utilizator).HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EchipamentConfiguration : IEntityTypeConfiguration<Echipament>
{
    public void Configure(EntityTypeBuilder<Echipament> b)
    {
        b.ToTable("echipamente");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.ComplexId).HasColumnName("complex_id").IsRequired();
        b.Property(x => x.Denumire).HasColumnName("denumire").IsRequired().HasMaxLength(200);
        b.Property(x => x.TipSport).HasColumnName("tip_sport").IsRequired().HasMaxLength(100);
        b.Property(x => x.StocTotal).HasColumnName("stoc_total").IsRequired();
        b.Property(x => x.TarifInchirierePerOra).HasColumnName("tarif_inchiriere_per_ora").IsRequired().HasColumnType("decimal(10,2)");

        b.HasOne(x => x.Complex).WithMany(x => x.Echipamente).HasForeignKey(x => x.ComplexId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RezervareConfiguration : IEntityTypeConfiguration<Rezervare>
{
    public void Configure(EntityTypeBuilder<Rezervare> b)
    {
        b.ToTable("rezervari");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.UtilizatorId).HasColumnName("utilizator_id").IsRequired();
        b.Property(x => x.TerenId).HasColumnName("teren_id").IsRequired();
        b.Property(x => x.CodRezervare).HasColumnName("cod_rezervare").IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.CodRezervare).IsUnique();
        b.Property(x => x.DataStart).HasColumnName("data_start").IsRequired();
        b.Property(x => x.DataEnd).HasColumnName("data_end").IsRequired();
        b.Property(x => x.DataExpirareBlocaj).HasColumnName("data_expirare_blocaj");
        b.Property(x => x.PretTotal).HasColumnName("pret_total").IsRequired().HasColumnType("decimal(10,2)");
        b.Property(x => x.DiscountAplicat).HasColumnName("discount_aplicat").IsRequired().HasColumnType("decimal(10,2)");
        b.Property(x => x.Stare).HasColumnName("stare").IsRequired().HasConversion<string>();

        b.HasOne(x => x.Utilizator).WithMany(x => x.Rezervari).HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Teren).WithMany(x => x.Rezervari).HasForeignKey(x => x.TerenId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RezervareEchipamentConfiguration : IEntityTypeConfiguration<RezervareEchipament>
{
    public void Configure(EntityTypeBuilder<RezervareEchipament> b)
    {
        b.ToTable("rezervare_echipamente");
        b.HasKey(x => new { x.RezervareId, x.EchipamentId });
        b.Property(x => x.RezervareId).HasColumnName("rezervare_id");
        b.Property(x => x.EchipamentId).HasColumnName("echipament_id");
        b.Property(x => x.Cantitate).HasColumnName("cantitate").IsRequired();
        b.Property(x => x.SubtotalPret).HasColumnName("subtotal_pret").IsRequired().HasColumnType("decimal(10,2)");

        b.HasOne(x => x.Rezervare).WithMany(x => x.RezervareEchipamente).HasForeignKey(x => x.RezervareId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Echipament).WithMany(x => x.RezervareEchipamente).HasForeignKey(x => x.EchipamentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AbonamentConfiguration : IEntityTypeConfiguration<Abonament>
{
    public void Configure(EntityTypeBuilder<Abonament> b)
    {
        b.ToTable("abonamente");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.UtilizatorId).HasColumnName("utilizator_id").IsRequired();
        b.Property(x => x.Tip).HasColumnName("tip").IsRequired().HasConversion<string>();
        b.Property(x => x.DataStart).HasColumnName("data_start").IsRequired();
        b.Property(x => x.DataExpirare).HasColumnName("data_expirare").IsRequired();
        b.Property(x => x.SedinteRamase).HasColumnName("sedinte_ramase").IsRequired();
        b.Property(x => x.Activ).HasColumnName("activ").IsRequired();

        b.HasOne(x => x.Utilizator).WithMany(x => x.Abonamente).HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PlataConfiguration : IEntityTypeConfiguration<Plata>
{
    public void Configure(EntityTypeBuilder<Plata> b)
    {
        b.ToTable("plati");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        b.Property(x => x.UtilizatorId).HasColumnName("utilizator_id").IsRequired();
        b.Property(x => x.RezervareId).HasColumnName("rezervare_id");
        b.Property(x => x.AbonamentId).HasColumnName("abonament_id");
        b.Property(x => x.StripeSessionId).HasColumnName("stripe_session_id").IsRequired().HasMaxLength(200);
        b.HasIndex(x => x.StripeSessionId).IsUnique();
        b.Property(x => x.Suma).HasColumnName("suma").IsRequired().HasColumnType("decimal(10,2)");
        b.Property(x => x.DataPlata).HasColumnName("data_plata").IsRequired();
        b.Property(x => x.Stare).HasColumnName("stare").IsRequired().HasMaxLength(50);
        b.Property(x => x.MetodaPlata).HasColumnName("metoda_plata").IsRequired().HasMaxLength(50);

        b.HasOne(x => x.Utilizator).WithMany(x => x.Plati).HasForeignKey(x => x.UtilizatorId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Rezervare).WithMany(x => x.Plati).HasForeignKey(x => x.RezervareId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Abonament).WithMany(x => x.Plati).HasForeignKey(x => x.AbonamentId).OnDelete(DeleteBehavior.SetNull);
    }
}
