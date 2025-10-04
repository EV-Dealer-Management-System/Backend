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

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<ContractTemplate> ContractTemplates { get; set; }
        public DbSet<Dealer> Dealers { get; set; }

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
        }
    }
}
