using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.DealerFeedBackDTO
{
    public class CreateDealerFeedBackDTO
    {
        public string? FeedbackContent { get; set; }
        public List<string> Key { get; set; }
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
    }
}
