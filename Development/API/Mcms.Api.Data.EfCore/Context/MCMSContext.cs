using System;
using System.Linq;
using Mcms.Api.Data.Core.Raw;
using Mcms.Api.Data.Poco.Models.Core;
using Mcms.Api.Data.Poco.Models.Core.Release;
using Mcms.Api.Data.Poco.Models.Mapping.Component;
using Mcms.Api.Data.Poco.Models.Mapping.Mappings;
using Mcms.Api.Data.Poco.Models.Mapping.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Mcms.Api.Data.EfCore.Context
{
    public class MCMSContext
        : DbContext, IRawDataAccessor
    {

        public MCMSContext(DbContextOptions<MCMSContext> options) : base(options)
        {
        }

        public DbSet<GameVersion> GameVersions { get; set; }

        public DbSet<Release> Releases { get; set; }

        public DbSet<MappingType> MappingTypes { get; set; }

        public DbSet<Component> Components { get; set; }

        public DbSet<VersionedComponent> VersionedComponents { get; set; }

        public DbSet<CommittedMapping> LiveMappingEntries { get; set; }

        public DbSet<ProposedMapping> ProposalMappingEntries { get; set; }

        public DbSet<ReleaseComponent> ReleaseComponents { get; set; }

        public DbSet<LockingEntry> LockingEntries { get; set; }

        public DbSet<MetadataBase> VersionedComponentMetadata { get; set; }

        public DbSet<ClassMetadata> ClassMetadata { get; set; }

        public DbSet<MethodMetadata> MethodMetadata { get; set; }

        public DbSet<FieldMetadata> FieldMetadata { get; set; }

        public DbSet<ParameterMetadata> ParameterMetadata { get; set; }

        public DbSet<PackageMetadata> PackageMetadata { get; set; }

        public DbSet<ClassInheritanceData> ClassInheritanceData { get; set; }

        public void MarkObjectChanged(object obj)
        {
            this.Entry(obj).State = EntityState.Modified;
        }

        public void MarkObjectAdded(object obj)
        {
            this.Entry(obj).State = EntityState.Added;
        }

        public void MarkObjectRemoved(object obj)
        {
            this.Entry(obj).State = EntityState.Deleted;
        }

        public void Detach(object obj)
        {
            this.Entry(obj).State = EntityState.Detached;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameVersion>()
                .HasIndex(version => version.Name)
                .IsUnique();

            modelBuilder.Entity<Release>()
                .HasIndex(release => release.Name)
                .IsUnique();

            modelBuilder.Entity<ClassMetadata>(builder =>
            {
                builder
                    .HasMany(metadata => metadata.InnerClasses).WithOne(metadata => metadata.Outer);
                builder
                    .HasMany(metadata => metadata.InheritsFrom).WithOne(data => data.Subclass);
                builder
                    .HasMany(metadata => metadata.IsInheritedBy).WithOne(data => data.Superclass);
            });
        }

        IQueryable<GameVersion> IRawDataAccessor.GameVersions => GameVersions;

        IQueryable<Release> IRawDataAccessor.Releases => Releases;

        IQueryable<MappingType> IRawDataAccessor.MappingTypes => MappingTypes;

        IQueryable<Component> IRawDataAccessor.Components => Components;

        IQueryable<VersionedComponent> IRawDataAccessor.VersionedComponents => VersionedComponents;

        IQueryable<CommittedMapping> IRawDataAccessor.LiveMappingEntries => LiveMappingEntries;

        IQueryable<ProposedMapping> IRawDataAccessor.ProposalMappingEntries => ProposalMappingEntries;

        IQueryable<ReleaseComponent> IRawDataAccessor.ReleaseComponents => ReleaseComponents;

        IQueryable<LockingEntry> IRawDataAccessor.LockingEntries => LockingEntries;

        IQueryable<MetadataBase> IRawDataAccessor.VersionedComponentMetadata => VersionedComponentMetadata;

        IQueryable<ClassMetadata> IRawDataAccessor.ClassMetadata => ClassMetadata;

        IQueryable<MethodMetadata> IRawDataAccessor.MethodMetadata => MethodMetadata;

        IQueryable<FieldMetadata> IRawDataAccessor.FieldMetadata => FieldMetadata;

        IQueryable<ParameterMetadata> IRawDataAccessor.ParameterMetadata => ParameterMetadata;

        IQueryable<PackageMetadata> IRawDataAccessor.PackageMetadata => PackageMetadata;
    }
}
