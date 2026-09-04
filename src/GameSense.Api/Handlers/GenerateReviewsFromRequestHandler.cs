using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Core.Services;
using GameSense.Core.Repositories;
using GameSense.Core.Exceptions;
using GameSense.Api.Requests;
using MediatR;

namespace GameSense.Api.Handlers
{
    public class GenerateReviewsFromRequestHandler : IRequestHandler<GenerateReviewsCommand, List<ReviewResponseDto>>
    {
        private readonly IReviewService _service;
        private readonly IMapper _mapper;
        private readonly IUserRepository _users;

        public GenerateReviewsFromRequestHandler(IReviewService service, IMapper mapper, IUserRepository users)
        {
            _service = service;
            _mapper = mapper;
            _users = users;
        }

        public async Task<List<ReviewResponseDto>> Handle(GenerateReviewsCommand command, CancellationToken cancellationToken)
        {
            var user = await _users.GetByIdAsync(command.UserId, cancellationToken);
            if (user?.ExpertiseScore is not decimal score || score < 80m)
                throw new ExpertiseEligibilityException("An ExpertiseScore of at least 80 is required to generate reviews.");

            var request = command.Request;
            var created = await _service.GenerateAndSaveReviewsAsync(request.FranchiseNames, request.QuestionsPerFranchise, request.PromptStyle, cancellationToken);
            return created.Select(r => _mapper.Map<ReviewResponseDto>(r)).ToList();
        }
    }
}
