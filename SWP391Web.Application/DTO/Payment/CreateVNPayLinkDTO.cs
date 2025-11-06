using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SWP391Web.Application.DTO.Payment
{
    public class CreateVNPayLinkDTO
    {
        public string Locale { get; set; } = null!;
    }
}
