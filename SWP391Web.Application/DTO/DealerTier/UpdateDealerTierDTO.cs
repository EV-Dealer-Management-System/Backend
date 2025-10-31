using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.DealerTier
{
    public class UpdateDealerTierDTO
    {
        [Required]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? Level { get; set; }
        public decimal? BaseCommissionPercent { get; set; }
        public decimal? BaseDepositPercent { get; set; }
        public decimal? BaseLatePenaltyPercent { get; set; }
        public decimal? BaseCreditLimit { get; set; }
        public string? Description { get; set; }
    }
}
