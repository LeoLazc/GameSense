using AutoMapper;
using GameSense.Api.DTOs;
using GameSense.Api.Handlers;
using GameSense.Api.Requests;
using GameSense.Core.Models;
using GameSense.Core.Repositories;
using Moq;
using NUnit.Framework;

namespace GameSense.Api.Tests.Handlers;

[TestFixture]
public sealed class GetQuizResultHandlerTests
{
    [Test]
    public async Task Returns_mapped_result_or_null_for_another_users_session()
    {
        var session = new QuizSession { Id = 4, UserId = 7 };
        var quizzes = new Mock<IQuizRepository>();
        quizzes.Setup(x => x.GetSessionAsync(4, 7, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        quizzes.Setup(x => x.GetSessionAsync(4, 8, It.IsAny<CancellationToken>())).ReturnsAsync((QuizSession?)null);
        var dto = new QuizResultDto(4, session.StartedAt, null, "", null, null, Array.Empty<QuizAnswerDto>());
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<QuizResultDto>(session)).Returns(dto);
        var handler = new GetQuizResultHandler(quizzes.Object, mapper.Object);

        Assert.That(await handler.Handle(new GetQuizResultQuery(7, 4), CancellationToken.None), Is.SameAs(dto));
        Assert.That(await handler.Handle(new GetQuizResultQuery(8, 4), CancellationToken.None), Is.Null);
    }
}
