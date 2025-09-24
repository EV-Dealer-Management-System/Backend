using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Domain.Entities;
using SWP391Web.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.Services
{
    public class ContractTemplateService : IContractTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ContractTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDTO> CreateContractTemplateAsync(string code, string name, CancellationToken token)
        {
            try
            {
                var template = await _unitOfWork.ContractTemplateRepository.GetbyCodeAsync(code, token);
                if (template is not null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Template already exists",
                        StatusCode = 404
                    };
                }

                ContractTemplate newTemplate = new ContractTemplate(code, name);
                await _unitOfWork.ContractTemplateRepository.AddAsync(newTemplate, token);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Template created successfully",
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = $"An error occurred at CreateContractTemplateAsync in ContractTemplateService: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
