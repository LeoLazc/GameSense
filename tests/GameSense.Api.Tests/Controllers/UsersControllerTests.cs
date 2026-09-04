using GameSense.Api.Controllers;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using GameSense.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Controllers;

[TestFixture]
public sealed class UsersControllerTests
{
    [Test]
    public async Task Maps_mediator_results_and_returns_not_found()
    {
        var mediator = new Mock<IMediator>();
        var users = new[] { new UserResponseDto(1, "alice", "alice@example.com", DateTime.UtcNow, 82m) };
        mediator.Setup(x => x.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(users);
        mediator.Setup(x => x.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserResponseDto?)null);
        var controller = new UsersController(mediator.Object);

        var listResult = await controller.GetAll(CancellationToken.None);
        var missingResult = await controller.GetById(9, CancellationToken.None);

        Assert.That(((OkObjectResult)listResult.Result!).Value, Is.SameAs(users));
        Assert.That(missingResult.Result, Is.TypeOf<NotFoundResult>());
    }
}
