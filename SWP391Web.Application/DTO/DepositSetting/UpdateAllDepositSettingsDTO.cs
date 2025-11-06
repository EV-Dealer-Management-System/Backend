using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.DepositSetting
{
    public class UpdateAllDepositSettingsDTO
    {
        public decimal? MinDepositPercentage { get; set; }
        public decimal? MaxDepositPercentage { get; set; }
    }
}
