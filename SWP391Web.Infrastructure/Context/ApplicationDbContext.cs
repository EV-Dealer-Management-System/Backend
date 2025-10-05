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
        public DbSet<EContractTemplate> ContractTemplates { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<ElectricVehicleColor> ElectricVehicleColors { get; set; }
        public DbSet<ElectricVehicleModel> ElectricVehicleModels { get; set; }
        public DbSet<ElectricVehicleVersion> ElectricVehicleVersions { get; set; }
        public DbSet<ElectricVehicle> ElectricVehicles { get; set; }
        public DbSet<EContract> EContracts { get; set; }
        public DbSet<EContractTemplate> EContractTemplates { get; set; }
        public DbSet<EContractTemplateVersion> EContractTemplateVersions { get; set; }
        public DbSet<EContractAmendment> EContractAmendments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            EmailSeeder.SeedEmailTemplate(modelBuilder);

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
                .HasOne(ev => ev.Dealer)
                .WithMany(d => d.ElectricVehicles)
                .HasForeignKey(ev => ev.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure ElectricVehicleVersion entity

            modelBuilder.Entity<ElectricVehicleVersion>()
                .HasOne(vs => vs.Model)
                .WithMany(ev => ev.Versions)
                .HasForeignKey(ev => ev.ModelId)
                .OnDelete(DeleteBehavior.Restrict);

            /******************************************************************************/
            // Configure EContractAmendment entity

            modelBuilder.Entity<EContractAmendment>()
                .HasOne(ea => ea.EContract)
                .WithMany(e => e.Amendments)
                .HasForeignKey(ea => ea.EContractId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
