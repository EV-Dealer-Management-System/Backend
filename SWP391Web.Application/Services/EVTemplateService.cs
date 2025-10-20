using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.ElectricVehicle;
using SWP391Web.Application.DTO.EVTemplate;
using SWP391Web.Application.DTO.Promotion;
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
    public class EVTemplateService : IEVTemplateService
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly IMapper _mapper;
        public readonly IS3Service _s3Service;

        public EVTemplateService(IUnitOfWork unitOfWork, IMapper mapper, IS3Service s3Service)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _s3Service = s3Service ?? throw new ArgumentNullException(nameof(_s3Service));
        }
        public async Task<ResponseDTO> CreateEVTemplateAsync(CreateEVTemplateDTO createEVTemplateDTO)
        {
            try
            {
                ElectricVehicleTemplate template = new ElectricVehicleTemplate
                {
                    VersionId = createEVTemplateDTO.VersionId,
                    ColorId = createEVTemplateDTO.ColorId,
                    Price = createEVTemplateDTO.Price,
                    Description = createEVTemplateDTO.Description
                };
                if(template == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Template is null.",
                        StatusCode = 404
                    };
                }

                foreach (var key in createEVTemplateDTO.AttachmentKeys)
                {
                    var fileName = Path.GetFileName(key);
                    template.EVAttachments.Add(new EVAttachment
                    {
                        FileName = fileName,
                        Key = key
                    });
                }

                await _unitOfWork.EVTemplateRepository.AddAsync(template,CancellationToken.None);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Template created successfully",
                    StatusCode = 200,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetAllVehicleTemplateAsync()
        {
            try
            {
                var templates = await _unitOfWork.EVTemplateRepository.GetAllAsync(
                    includes: q => q
                        .Include(t => t.Version)
                            .ThenInclude(v => v.Model)
                        .Include(t => t.Color));
                        //.Include(t => t.EVAttachments));
                var getTemples = _mapper.Map<List<GetEVTemplateDTO>>(templates);

                foreach (var tem in getTemples)
                {
                    var attachments = _unitOfWork.EVAttachmentRepository
                        .GetAttachmentsByElectricVehicleTemplateId(tem.Id);

                    var urlList = new List<string>();
                    foreach (var att in attachments)
                    {
                        var url = _s3Service.GenerateDownloadUrl(att.Key);
                        urlList.Add(url);
                    }

                    tem.ImgUrl = urlList;
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Get all Template successfully",
                    StatusCode = 200,
                    Result = getTemples
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> GetVehicleTemplateByIdAsync(Guid EVTemplateId)
        {
            try
            {
                var template = await _unitOfWork.EVTemplateRepository.Query(t => t.Id == EVTemplateId)
                    .Include(t => t.Version)
                        .ThenInclude(v => v.Model)
                    .Include(t => t.Color)
                    //.Include(t => t.EVAttachments)
                    .FirstOrDefaultAsync();
                if (template == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = "Template not exist",
                        StatusCode = 404
                    };
                }

                var getTemplate = _mapper.Map<GetEVTemplateDTO>(template);
                if (template.EVAttachments != null && template.EVAttachments.Any())
                {
                    getTemplate.ImgUrl = template.EVAttachments
                        .Select(a => _s3Service.GenerateDownloadUrl(a.Key))
                        .ToList();
                }

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = " Get Template successfully",
                    StatusCode = 200,
                    Result = getTemplate
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }

        public async Task<ResponseDTO> UpdateEVTemplateAsync(Guid EVTemplateId,UpdateEVTemplateDTO updateEVTemplateDTO)
        {
            try
            {
                var template = await _unitOfWork.EVTemplateRepository.GetByIdAsync(EVTemplateId);
                if(template == null)
                {
                    return new ResponseDTO
                    {
                        IsSuccess = false,
                        Message = " Template not found",
                        StatusCode = 404
                    };
                }

                if (!string.IsNullOrWhiteSpace(updateEVTemplateDTO.Description))
                {
                    template.Description = updateEVTemplateDTO.Description.Trim();
                }
                if (updateEVTemplateDTO.Price.HasValue && updateEVTemplateDTO.Price.Value >= 0)
                    template.Price = updateEVTemplateDTO.Price.Value;

                //Take photo
                if (updateEVTemplateDTO.AttachmentKeys != null && updateEVTemplateDTO.AttachmentKeys.Any())
                {
                    //Remove all photo
                    foreach (var att in template.EVAttachments.ToList())
                    {
                        _unitOfWork.EVAttachmentRepository.Remove(att);
                    }

                    template.EVAttachments.Clear();

                    //Add new photo
                    foreach (var key in updateEVTemplateDTO.AttachmentKeys)
                    {
                        var fileName = Path.GetFileName(key);
                        template.EVAttachments.Add(new EVAttachment
                        {
                            ElectricVehicleTemplateId = template.Id,
                            FileName = fileName,
                            Key = key
                        });
                    }
                }

                _unitOfWork.EVTemplateRepository.Update(template);
                await _unitOfWork.SaveAsync();

                return new ResponseDTO
                {
                    IsSuccess = true,
                    Message = "Template updated successfully",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    StatusCode = 500
                };
            }
        }
    }
}
