using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Repositories;
using MediatR;

namespace GameSense.Api.Handlers
{
    public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserResponseDto?>
    {
        private readonly IUserRepository _users;
        private readonly IMapper _mapper;

        public GetUserByIdHandler(IUserRepository users, IMapper mapper)
        {
            _users = users;
            _mapper = mapper;
        }

        public async Task<UserResponseDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _users.GetByIdAsync(request.Id, cancellationToken);
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
        }
    }
}
