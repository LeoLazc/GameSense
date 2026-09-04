using GameSense.Api.DTOs;
using MediatR;

namespace GameSense.Api.Requests
{
    public class GetReviewQuery : IRequest<ReviewResponseDto>
    {
        public int Id { get; set; }
    }
}
