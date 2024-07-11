using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Cnbv.ConectaProcesos.Opiniones.Data.DatabaseConectaProcesos;

public partial class ConectaProcesosRefactorContext : DbContext
{
    public ConectaProcesosRefactorContext(DbContextOptions<ConectaProcesosRefactorContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ArchivoOpinion> ArchivoOpinions { get; set; }

    public virtual DbSet<Opinion> Opinions { get; set; }

    public virtual DbSet<OpinionReceptor> OpinionReceptors { get; set; }

    public virtual DbSet<TipoDocumentoEnum> TipoDocumentoEnums { get; set; }

    public virtual DbSet<TipoElementoEnum> TipoElementoEnums { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Modern_Spanish_CI_AS");

        modelBuilder.Entity<ArchivoOpinion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ArchivoOpinion2");

            entity.ToTable("ArchivoOpinion", "Opinion");

            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Ruta)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.HasOne(d => d.IdOpinionNavigation).WithMany(p => p.ArchivoOpinions)
                .HasForeignKey(d => d.IdOpinion)
                .HasConstraintName("FK_ArchivoOpinion_Opinion");

            entity.HasOne(d => d.IdReceptorNavigation).WithMany(p => p.ArchivoOpinions)
                .HasForeignKey(d => d.IdReceptor)
                .HasConstraintName("FK_ArchivoOpinion_OpinionReceptor");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.ArchivoOpinions)
                .HasForeignKey(d => d.IdTipoDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchivoOpinion_TipoDocumentoEnum");

            entity.HasOne(d => d.IdTipoElementoNavigation).WithMany(p => p.ArchivoOpinions)
                .HasForeignKey(d => d.IdTipoElemento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchivoOpinion_TipoElementoEnum");
        });

        modelBuilder.Entity<Opinion>(entity =>
        {
            entity.ToTable("Opinion", "Opinion");

            entity.Property(e => e.CadenaOriginal)
                .IsUnicode(false)
                .HasDefaultValueSql("('N/A')");
            entity.Property(e => e.Detalle).IsUnicode(false);
            entity.Property(e => e.FechaSolicitud).HasColumnType("datetime");
            entity.Property(e => e.FolioAsunto)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<OpinionReceptor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OpinionReceptor2");

            entity.ToTable("OpinionReceptor", "Opinion");

            entity.Property(e => e.CadenaOriginal).IsUnicode(false);
            entity.Property(e => e.Clave)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ComentarioFirmante)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Comentarios).IsUnicode(false);
            entity.Property(e => e.EstatusSolicitud)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FechaRespuesta).HasColumnType("datetime");
            entity.Property(e => e.FinalizadaPor)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Firmante)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IdEnvio)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Nombre).IsUnicode(false);

            entity.HasOne(d => d.IdOpinionNavigation).WithMany(p => p.OpinionReceptors)
                .HasForeignKey(d => d.IdOpinion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OpinionReceptor_Opinion");
        });

        modelBuilder.Entity<TipoDocumentoEnum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoDocu__3214EC07679DB4AE");

            entity.ToTable("TipoDocumentoEnum", "Catalogo");

            entity.Property(e => e.Nombre)
                .HasMaxLength(45)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TipoElementoEnum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TipoElem__3214EC074356FF90");

            entity.ToTable("TipoElementoEnum", "Catalogo");

            entity.Property(e => e.Nombre)
                .HasMaxLength(45)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
