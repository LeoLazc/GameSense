using System.Security.Claims;
using GameSense.Api.Controllers;
using GameSense.Api.DTOs;
using GameSense.Api.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Controllers;

[TestFixture]
public sealed class QuizControllerTests
{
    [Test]
    public async Task Uses_authenticated_user_and_returns_mediator_result()
    {
        var mediator = new Mock<IMediator>();
        var session = new QuizSessionDto(4, DateTime.UtcNow, null, "InProgress", null, Array.Empty<QuizQuestionDto>(), new QuizProgressDto(0, 1));
        mediator.Setup(x => x.Send(It.Is<StartQuizCommand>(c => c.UserId == 12), It.IsAny<CancellationToken>())).ReturnsAsync(session);
        var controller = new QuizController(mediator.Object) { ControllerContext = ContextForUser(12) };

        var result = await controller.Start(CancellationToken.None);

        Assert.That(((OkObjectResult)result.Result!).Value, Is.SameAs(session));
        mediator.Verify(x => x.Send(It.Is<StartQuizCommand>(c => c.UserId == 12), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ControllerContext ContextForUser(int id) => new()
    {
        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()) })) }
    };
}
