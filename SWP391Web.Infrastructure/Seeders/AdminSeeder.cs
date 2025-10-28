using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SWP391Web.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Infrastructure.Seeders
{
    public class AdminSeeder
    {
        public static void AdminConfigure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepositSetting>().HasData(
                new
                {
                    Id = Guid.Parse("6a574799-34f4-46a2-9843-09e9dd3e4bcf"),
                    ManagerId = "11582b41-2fde-4c54-a978-c181fd71bd6c",
                    MinDepositPercentage = 2.0m,
                    MaxDepositPercentage = 6.0m,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
