﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IRedisService
    {
        Task<bool> StoreKeyAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> RetrieveString(string key);
    }
}
