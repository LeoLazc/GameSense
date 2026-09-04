using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Infrastructure.Data;
using MediatR;

namespace GameSense.Api.Handlers
{
    public class GetReviewHandler : IRequestHandler<GetReviewQuery, ReviewResponseDto>
    {
        private readonly GameSenseDbContext _db;
        private readonly IMapper _mapper;

        public GetReviewHandler(GameSenseDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ReviewResponseDto> Handle(GetReviewQuery request, CancellationToken cancellationToken)
        {
            var review = await _db.Reviews.FindAsync(new object[] { request.Id }, cancellationToken);
            if (review == null) return null!;
            await _db.Entry(review).Reference(r => r.Franchise).LoadAsync(cancellationToken);
            return _mapper.Map<ReviewResponseDto>(review);
        }
    }
}
