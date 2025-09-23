using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class ContractLineItem
    {
        public Guid Id { get; set; }
        public string ModelCode { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public List<string> VinOrSerials { get; set; } = new();
        public ContractLineItem(string model, int quantity, decimal price, IEnumerable<string>? vins = null)
        {
            ModelCode = model;
            Quantity = quantity;
            Price = price;
            if (vins is not null)
            {
                VinOrSerials.AddRange(vins);
            }
        }
    }
}
