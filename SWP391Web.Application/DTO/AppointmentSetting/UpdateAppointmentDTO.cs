﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.DTO.AppointmentSetting
{
    public class UpdateAppointmentDTO
    {
        public bool? AllowOverlappingAppointments { get; set; }
        public int? MaxConcurrentAppointments { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public int? MinIntervalBetweenAppointments { get; set; }
    }
}
