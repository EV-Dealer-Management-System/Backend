using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.DealerFeedBackDTO
{
    public class GetDealerFeedBackDTO
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public string? DealerName { get; set; }
        public string? FeedbackContent { get; set; }
        public List<string?> Key { get; set; }
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
        public DateTime CreatedAt { get; set; }
    }
}
