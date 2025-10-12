﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.EVCInventory
{
    public class CreateEVCInventoryDTO
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
