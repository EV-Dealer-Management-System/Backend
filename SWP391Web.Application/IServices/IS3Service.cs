using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.DTO.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IServices
{
    public interface IS3Service
    {
        ResponseDTO GenerateUploadUrl(string objectKey, string contentType);
        string GenerateDownloadUrl(string objectKey);
        ResponseDTO GenerateUploadElectricVehicle(PreSignedUploadDTO preSignedUploadDTO);
        ResponseDTO GenerateUploadEcontract(PreSignedUploadDTO preSignedUploadDTO);
    }
}
