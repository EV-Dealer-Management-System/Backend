using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.DealerFeedBackDTO;
using SWP391Web.Application.DTO.S3;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerFeedbackController : ControllerBase
    {
        public readonly IDealerFeedbackService _dealerFeedbackService;
        public readonly IS3Service _s3Service;
        public DealerFeedbackController(IDealerFeedbackService dealerFeedbackService, IS3Service s3Service)
        {
            _dealerFeedbackService = dealerFeedbackService;
            _s3Service = s3Service;
        }
        [HttpPost("CreateDealerFeedback")]
        public async Task<ActionResult<ResponseDTO>> CreateDealerFeedback([FromBody]CreateDealerFeedBackDTO createDealerFeedBackDTO)
        {
            var response = await _dealerFeedbackService.CreateDealerFeedbackAsync(User, createDealerFeedBackDTO);
            return StatusCode(response.StatusCode, response);
            
        }
        [HttpPost("upload-file-url-dealer-feedback")]
        public ActionResult<ResponseDTO> UploadFileUrlDealerFeedbackAsync([FromBody] PreSignedUploadDTO preSignedUploadDTO)
        {
            var response = _s3Service.GenerateUploadDealerFBAttachment(preSignedUploadDTO);
            return StatusCode(response.StatusCode, response);
        }
    }
}
