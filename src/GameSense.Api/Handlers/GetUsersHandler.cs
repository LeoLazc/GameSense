using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Repositories;
using MediatR;

namespace GameSense.Api.Handlers
{
    public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserResponseDto>>
    {
        private readonly IUserRepository _users;
        private readonly IMapper _mapper;

        public GetUsersHandler(IUserRepository users, IMapper mapper)
        {
            _users = users;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<UserResponseDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _users.GetAllAsync(cancellationToken);
            return _mapper.Map<List<UserResponseDto>>(users);
        }
    }
}
