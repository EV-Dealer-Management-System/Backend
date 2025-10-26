using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class AppointmentSetting
    {
        public Guid Id { get; set; }
        public string ManagerId { get; set; } = null!;
        public Guid? DealerId { get; set; }
        public bool AllowOverlappingAppointments { get; set; }
        public int MaxConcurrentAppointments { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public int MinIntervalBetweenAppointments { get; set; }
        public DateTime CreatedAt { get; set; }

        public ApplicationUser Manager { get; set; } = null!;
        public Dealer? Dealer { get; set; }
    }
}
