﻿using DomainDrivenWebApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace DomainDrivenWebApplication.Infrastructure.Data;

/// <summary>
/// Represents the database context for interacting with schools data.
/// </summary>
public sealed class SchoolContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchoolContext"/> class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
    {
        if (Database.GetService<IDatabaseCreator>() is not RelationalDatabaseCreator dbCreate) return;
        // Create Database 
        if (!dbCreate.CanConnect())
        {
            dbCreate.Create();
        }

        // Create Tables
        if (!dbCreate.HasTables())
        {
            dbCreate.CreateTables();
        }
    }

    /// <summary>
    /// Gets or sets the DbSet of schools.
    /// </summary>
    public DbSet<School> Schools { get; set; }

    /// <summary>
    /// Configures the database model for the School entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance used to construct the model for the context being created.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<School>(ConfigureSchool);
    }

    /// <summary>
    /// Configures the School entity properties and mappings.
    /// </summary>
    /// <param name="entity">The entity type builder for the School entity.</param>
    private static void ConfigureSchool(EntityTypeBuilder<School> entity)
    {
        entity.ToTable("Schools", b => b.IsTemporal(
            temporalTableBuilder =>
            {
                temporalTableBuilder.HasPeriodStart("ValidFrom");  // Specifies the start of the system versioning period.
                temporalTableBuilder.HasPeriodEnd("ValidTo");      // Specifies the end of the system versioning period.
                temporalTableBuilder.UseHistoryTable("SchoolHistoricalData"); // Specifies the name of the history table for storing historical data.
            }));

        entity.HasKey(e => e.Id);                           // Configures the primary key for the School entity.
        entity.Property(e => e.Id).ValueGeneratedOnAdd();   // Configures Id property to be generated on add.
        entity.Property(e => e.Name).IsRequired();          // Configures Name property to be required (not nullable).
        entity.Property(e => e.Address).IsRequired();       // Configures Address property to be required (not nullable).
        entity.Property(e => e.PrincipalName).IsRequired(); // Configures PrincipalName property to be required (not nullable).
    }

    /// <summary>
    /// Configures warnings and behaviors for EF Core.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Log(RelationalEventId.PendingModelChangesWarning));
        base.OnConfiguring(optionsBuilder);
    }
}
