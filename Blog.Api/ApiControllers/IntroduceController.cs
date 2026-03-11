using MicroBase.Dashboard.Share.Models;
using MicroBase.Service.Settings;
using MicroBase.Share.Extensions;
using MicroBase.Share.Models;
using MicroBase.Share.Models.Base;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.ApiControllers
{
    [ApiController]
    [Route("api-v1/setting")]
    public class IntroduceController : ControllerBase
    {
        private readonly ISiteSettingService siteSettingService;
        public IntroduceController(
            ISiteSettingService siteSettingService)
        {
            this.siteSettingService = siteSettingService;
        }

        [HttpGet]
        public async Task<BaseResponse<SiteSettingResponse>> GetByKey(string key)
        {
            var res = await siteSettingService.GetByKeyAsync(key, true);
            if (res == null || !res.Success || res.Data == null || string.IsNullOrWhiteSpace(res.Data.StringValue))
            {
                return new BaseResponse<SiteSettingResponse>
                {
                    Success = true,
                    Data = new SiteSettingResponse()
                };
            }

            var model = JsonExtensions.JsonDeserialize<StaticContentModel>(res.Data.StringValue);
            return new BaseResponse<SiteSettingResponse>
            {
                Success = true,
                Data = new SiteSettingResponse
                {
                    StringValue = model.StringValue
                }
            };
        }
    }
}
