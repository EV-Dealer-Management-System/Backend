using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Log
    {
        public Guid Id { get; set; }
        public Guid? DealerId { get; set; }
        public string ManagerId { get; set; } = null!;
        public string? Description { get; set; }
        public string? EntityName { get; set; }
        public LogType LogType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
