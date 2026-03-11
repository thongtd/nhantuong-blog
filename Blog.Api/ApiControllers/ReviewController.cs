using MicroBase.Entity.Entities;
using MicroBase.Service.Review;
using MicroBase.Share.Models.Base;
using MicroBase.Share.Models.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.ApiControllers
{
    [ApiController]
    [Route("api-v1/review")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService reviewService;
        public ReviewController(
            IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<BaseResponse<List<ReviewResponse>>> GetListReview(int? rating, string? reviewerName)
        {
            return await reviewService.GetListReview(rating, reviewerName);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<BaseResponse<bool>> AddReview(ProductReview productReview)
        {
            return await reviewService.AddReview(productReview);
        }
    }
}