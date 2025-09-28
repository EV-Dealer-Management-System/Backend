﻿using SWP391Web.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class VnptProcess
    {
        public string? Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ComId { get; set; }
        public bool IsOrder { get; set; }
        public int OrderNo { get; set; }
        public int PageSign { get; set; }
        public string? Position { get; set; }
        public VnptStatusDto? DisplayType { get; set; }
        public VnptStatusDto? AccessPermission { get; set; }
        public VnptStatusDto? Status { get; set; }
        public int ProcessedByUserId { get; set; }
        public string? DocumentId { get; set; }
        public List<object>? FillingItems { get; set; }
    }
}
