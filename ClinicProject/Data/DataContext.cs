using System;
using System.Collections.Generic;
using Core.DTOs;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public partial class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database= ClinicDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PriorityCity>().HasNoKey().ToView(null);
        modelBuilder.Entity <CountVisitToDoctor>().HasNoKey().ToView(null);
        modelBuilder.Entity <ClinicLoadDTO>().HasNoKey().ToView(null);
        modelBuilder.Entity <AppointmentDTO>().HasNoKey().ToView(null);
        modelBuilder.Entity<AvgHourToDoctorInMonth>().HasNoKey().ToView("AvgHourToDoctorInMonth");
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PK__Clients__E67E1A24C0DA3E8C");

            entity.Property(e => e.ClientId).ValueGeneratedNever();
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PK__Doctors__2DC00EBF9882904A");

            entity.Property(e => e.DoctorId).ValueGeneratedNever();
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Specialization).HasMaxLength(50);
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.VisitId).HasName("PK__Visits__4D3AA1DEC0A008F7");

            entity.Property(e => e.DateAndHour).HasColumnType("datetime");

            entity.HasOne(d => d.Client).WithMany(p => p.Visits)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visits_Clients");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Visits)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visits_Doctors");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
