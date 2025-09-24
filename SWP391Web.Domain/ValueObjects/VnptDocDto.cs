using SWP391Web.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.Auth
{
    public class VnptDocDto
    {
        public string Id { get; init; } = default!;
        public string No { get; init; } = default!;
        public string Subject { get; init; } = default!;
        public string? Description { get; init; }
        public string? DownloadUrl { get; init; }
        public VnptStatusDto Status { get; init; } = default!;
    }
}
