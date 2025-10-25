﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }

        public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
        public ICollection<Dealer> Dealers { get; set; } = new List<Dealer>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
