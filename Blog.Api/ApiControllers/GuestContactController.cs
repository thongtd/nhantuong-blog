using MicroBase.Service.API.GuestContact;
using MicroBase.Share.Models.API.GuestContact;
using MicroBase.Share.Models.Base;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.ApiControllers
{
    [ApiController]
    [Route("api-v1/guest-contact")]
    public class GuestContactController : ControllerBase
    {
        private readonly IGuestContactService guestContactService;
        public GuestContactController(IGuestContactService guestContactService)
        {
            this.guestContactService = guestContactService;
        }

        [HttpPost]
        public async Task<BaseResponse<bool>> CreateRequest(GuestContactModel model)
        {
            return await guestContactService.CreateRequest(model);
        }
    }
}
