using SWP391Web.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.CustomerFeedback
{
    public class CreateCustomerFeedbackDTO
    {
        public Guid CustomerId { get; set; }
        public string? FeedbackContent { get; set; }
        public List<string> AttachmentKeys { get; set; } = new();
        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
    }
}
