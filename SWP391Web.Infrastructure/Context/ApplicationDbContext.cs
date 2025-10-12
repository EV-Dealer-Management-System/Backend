using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.Seeders;

namespace SWP391Web.Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<ElectricVehicleColor> ElectricVehicleColors { get; set; }
        public DbSet<ElectricVehicleModel> ElectricVehicleModels { get; set; }
        public DbSet<ElectricVehicleVersion> ElectricVehicleVersions { get; set; }
        public DbSet<ElectricVehicle> ElectricVehicles { get; set; }
        public DbSet<EContract> EContracts { get; set; }
        public DbSet<EContractTemplate> EContractTemplates { get; set; }
        public DbSet<EContractTerm> EContractTerms { get; set; }
        public DbSet<BookingEV> BookingEVs { get; set; }
        public DbSet<BookingEVDetail> BookingEVDetails { get; set; }
        public DbSet<EVInventory> EVCInventories { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Seed initial data
            EmailSeeder.SeedEmailTemplate(modelBuilder);
            EContractSeeder.EContractTemplateSeeder.SeedDealerEContract(modelBuilder);
            EContractTermSeeder.SeedTerm(modelBuilder);

            // Customize ASP.NET Identity table names
            modelBuilder.Entity<ApplicationUser>(b =>
                b.ToTable("ApplicationUsers"));

            modelBuilder.Entity<IdentityRole>(b =>
                b.ToTable("Roles"));

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
                b.ToTable("UserRoles"));

            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
                b.ToTable("RoleClaims"));

            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
                b.ToTable("UserClaims"));

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
                b.ToTable("UserLogins"));

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
                b.ToTable("UserTokens"));


            modelBuilder.Entity<Dealer>()
                .HasMany(dl => dl.ApplicationUsers)
                .WithMany(au => au.Dealers)
                .UsingEntity<Dictionary<string, string>>(
                    "DealerMembers",
                    j => j
                        .HasOne<ApplicationUser>()
                        .WithMany()
                        .HasForeignKey("ApplicationUserId")
                        .HasConstraintName("FK_DealerMember_ApplicationUsers_ApplicationUserId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j => j
                        .HasOne<Dealer>()
                        .WithMany()
                        .HasForeignKey("DealerId")
                        .HasConstraintName("FK_DealerMember_Dealers_DealerId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j =>
                    {
                        j.HasKey("DealerId", "ApplicationUserId");
                        j.ToTable("DealerMembers");
                    });

            /******************************************************************************/
            // Configure Dealer entity

            // Dealer - ApplicationUser (many-to-many) relationship
            modelBuilder.Entity<Dealer>()
                .HasMany(d => d.ApplicationUsers)
                .WithMany(u => u.Dealers)
                .UsingEntity<Dictionary<string, object>>(
                    "DealerMembers",
                    j => j
                        .HasOne<ApplicationUser>()
                        .WithMany()
                        .HasForeignKey("ApplicationUserId")
                        .HasConstraintName("FK_DealerMembers_ApplicationUsers_ApplicationUserId")
                        .OnDelete(DeleteBehavior.Restrict),

                    j => j
                        .HasOne<Dealer>()
                        .WithMany()
                        .HasForeignKey("DealerId")
                        .HasConstraintName("FK_DealerMembers_Dealers_DealerId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j =>
                    {
                        j.HasKey("DealerId", "ApplicationUserId");
                        j.Property<Guid>("DealerId");
                        j.Property<string>("ApplicationUserId");
                        j.ToTable("DealerMembers");
                        j.HasIndex("DealerId");
                        j.HasIndex("ApplicationUserId");
                    });

            // Dealer - Manager (ApplicationUser) one-to-many relationship
            modelBuilder.Entity<Dealer>()
                .HasOne(d => d.Manager)
                .WithMany(m => m.ManagingDealers)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index on ManagerId in Dealer for performance
            modelBuilder.Entity<Dealer>()
                .HasIndex(d => d.ManagerId);

            /******************************************************************************/
            // Configure ElectricVehicle entity

            modelBuilder.Entity<ElectricVehicle>()
                .HasOne(ev => ev.Version)
                .WithMany(vs => vs.ElectricVehicles)
                .HasForeignKey(ev => ev.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ElectricVehicle>()
                .HasOne(ev => ev.Color)
                .WithMany(c => c.ElectricVehicles)
                .HasForeignKey(ev => ev.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ElectricVehicle>()
                .HasOne(ev => ev.Warehouse)
                .WithMany(d => d.ElectricVehicles)
                .HasForeignKey(ev => ev.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ElectricVehicle>()
                .HasIndex(ev => ev.VIN)
                .IsUnique();

            /******************************************************************************/
            // Configure ElectricVehicleVersion entity

            modelBuilder.Entity<ElectricVehicleVersion>()
                .HasOne(vs => vs.Model)
                .WithMany(ev => ev.Versions)
                .HasForeignKey(ev => ev.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure BookingEVDetail entity

            modelBuilder.Entity<BookingEVDetail>()
                .HasOne(bd => bd.BookingEV)
                .WithMany(b => b.BookingEVDetails)
                .HasForeignKey(bd => bd.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingEVDetail>()
                .HasOne(bd => bd.Version)
                .WithMany(v => v.BookingEVDetails)
                .HasForeignKey(bd => bd.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingEVDetail>()
                .HasOne(bd => bd.Color)
                .WithMany(c => c.BookingEVDetails)
                .HasForeignKey(bd => bd.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure BookingEV entity

            modelBuilder.Entity<BookingEV>()
                .HasOne(b => b.Dealer)
                .WithMany(d => d.BookingEVs)
                .HasForeignKey(b => b.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure EContract entity

            modelBuilder.Entity<EContract>()
                .HasOne(e => e.Ower)
                .WithOne(o => o.EContract)
                .HasForeignKey<EContract>(e => e.OwnerBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EContract>()
                .HasOne(e => e.EContractTemplate)
                .WithMany(t => t.EContracts)
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure Warehouse entity

            modelBuilder.Entity<Warehouse>()
                .HasOne(w => w.Dealer)
                .WithOne(d => d.Warehouse)
                .HasForeignKey<Warehouse>(w => w.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Warehouse>()
                .HasOne(w => w.EVInventory)
                .WithOne(d => d.Warehouse)
                .HasForeignKey<Warehouse>(w => w.EVInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
