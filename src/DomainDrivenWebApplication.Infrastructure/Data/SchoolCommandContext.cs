﻿using DomainDrivenWebApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainDrivenWebApplication.Infrastructure.Data
{
    /// <summary>
    /// Represents the database context for handling write operations on schools data.
    /// </summary>
    public sealed class SchoolCommandContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchoolCommandContext"/> class.
        /// </summary>
        /// <param name="options">The DbContext options.</param>
        public SchoolCommandContext(DbContextOptions<SchoolCommandContext> options) : base(options) { }

        /// <summary>
        /// Gets or sets the DbSet of schools for write operations.
        /// </summary>
        public DbSet<School> Schools { get; set; }

        /// <summary>
        /// Configures the database model for the School entity.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance used to construct the model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<School>(entity =>
            {
                entity.ToTable("Schools", b => b.IsTemporal(temporalTableBuilder =>
                {
                    temporalTableBuilder.HasPeriodStart("ValidFrom");
                    temporalTableBuilder.HasPeriodEnd("ValidTo");
                    temporalTableBuilder.UseHistoryTable("SchoolHistoricalData");
                }));

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Address).IsRequired();
                entity.Property(e => e.PrincipalName).IsRequired();
            });
        }
    }
}