using GameSense.Api.DTOs;
using MediatR;

namespace GameSense.Api.Requests
{
    public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserResponseDto>>;
}
