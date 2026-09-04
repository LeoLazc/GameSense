using GameSense.Api.DTOs;
using MediatR;

namespace GameSense.Api.Requests
{
    public sealed record GetUserByIdQuery(int Id) : IRequest<UserResponseDto?>;
}
