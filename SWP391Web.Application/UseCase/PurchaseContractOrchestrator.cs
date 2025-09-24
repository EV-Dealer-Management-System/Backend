using SWP391Web.Application.DTO.Auth;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.UseCase
{
    public sealed class PurchaseContractOrchestrator
    {
        private readonly IUnitOfWork _unitOfWork;
        public PurchaseContractOrchestrator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> CreateFormOrderAsync(Guid orderId, string templateType = "Purchase")
        {
            //try
            //{
            //    var customerOrder = await _unitOfWork.CustomerOrderRepository.GetByIdAsync(orderId);
            //    if (customerOrder is null)
            //    {
            //        return new ResponseDTO
            //        {
            //            IsSuccess = false,
            //            Message = "Customer order not found",
            //            StatusCode = 404
            //        };
            //    }


            //}

            throw new NotImplementedException();
        }

        private (byte[] bytes, string fileName) BuildPdfFromOrder(CustomerOrder order)
        {
            //TODO: render bằng QuestPDF → return (bytes, $"HD-{order.OrderNo}.pdf");
            throw new NotImplementedException();
        }
    }
}
