using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;
using SWP391Web.Infrastructure.IRepository;

namespace SWP391Web.Application.Services
{
    public class S3Service : IS3Service
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _bucketName;
        public S3Service(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _bucketName = config["S3Bucket:bucketName"] ?? throw new ArgumentNullException("S3Bucket:bucketName");
        }

        public string GenerateDownloadUrl(string objectKey)
        {
            var s3Client = new AmazonS3Client(RegionEndpoint.APNortheast1);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(5)
            };

            var url = s3Client.GetPreSignedURL(request);

            return url;
        }

        public ResponseDTO GenerateUploadUrl(string objectKey, string contentType)
        {
            var s3Client = new AmazonS3Client(RegionEndpoint.APSoutheast1);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(5),
                ContentType = contentType
            };

            var url = s3Client.GetPreSignedURL(request);
            return new ResponseDTO
            {
                IsSuccess = true,
                Message = "Generate upload URL successfully",
                StatusCode = 200,
                Result = new
                {
                    UploadUrl = url,
                    ObjectKey = objectKey
                }
            };
        }
    }
}
