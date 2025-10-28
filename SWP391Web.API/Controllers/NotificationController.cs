using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391Web.Application.DTO.Auth;
using SWP391Web.Application.IServices;

namespace SWP391Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [Route("get-all-notification")]
        public async Task<ActionResult<ResponseDTO>> GetAllNotification(CancellationToken ct)
        {
            var r = await _notificationService.GetAllNotification(User, ct);
            return StatusCode(r.StatusCode, r);
        }
    }
}
