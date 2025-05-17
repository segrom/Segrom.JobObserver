using Moq;
using Segrom.JobObserver.OzonService.TestUtils;
using Segrom.JobObserver.TelegramBot.Application;
using Segrom.JobObserver.TelegramBot.Application.Abstractions;
using Segrom.JobObserver.TelegramBot.Domain;
using Xunit.Abstractions;

namespace Segrom.JobObserver.TelegramBot.Test;

public class BotServiceUnitTests
{
	private readonly Mock<IMessagesRepository> _repositoryMock = new();
	private readonly Mock<IBotClient> _botMock = new();
	private readonly Mock<IOzonServiceApiClient> _vacanciesApiMock = new();
	private readonly BotService _sut;

	public BotServiceUnitTests(ITestOutputHelper output)
	{
		var loggerMock = new XUnitLogger<BotService>(output);
		_sut = new BotService(
			_repositoryMock.Object,
			_botMock.Object,
			_vacanciesApiMock.Object,
			loggerMock
		);
	}

	[Fact]
	public async Task HandleNewVacancies__Should_SendMessagesToAllChats_And_SaveThem()
	{
		// Arrange
		var chats = new List<long> { 1, 2 };
		var vacancies = new List<Vacancy> { new() { Id = 10 }, new() { Id = 20 } };
		const int resultMessageId = 1000;
		var cancellationToken = CancellationToken.None;

		_repositoryMock.Setup(x => x.GetAllChats(It.IsAny<CancellationToken>()))
			.ReturnsAsync(chats);

		_botMock.Setup(x => x.SendNewVacancyMessage(It.IsAny<Vacancy>(), It.IsAny<long>(), cancellationToken))
			.ReturnsAsync(() => resultMessageId);

		// Act
		await _sut.HandleNewVacancies(vacancies, cancellationToken);

		// Assert
		_repositoryMock.Verify(x => x.GetAllChats(cancellationToken), Times.Once);
		
		_botMock.Verify(x => x.SendNewVacancyMessage(It.IsAny<Vacancy>(), It.IsAny<long>(), cancellationToken),
			Times.Exactly(chats.Count * vacancies.Count));

		_repositoryMock.Verify(x => x.AddMessages(
			It.Is<List<VacancyMessage>>(messages =>
				messages.Count == chats.Count * vacancies.Count &&
				messages.TrueForAll(m => m.VacancyId == 10 || m.VacancyId == 20)),
			cancellationToken
		), Times.Once);
	}

	[Fact]
	public async Task HandleNewVacancies__Should_ThrowAppException()
	{
		// Arrange
		var ex = new Exception("Test");
		_repositoryMock.Setup(x => x.GetAllChats(It.IsAny<CancellationToken>()))
			.ThrowsAsync(ex);

		// Act & 
		var exception = await Record.ExceptionAsync(() =>
			_sut.HandleNewVacancies(new List<Vacancy>(), CancellationToken.None));
		
		// Assert
		Assert.NotNull(exception);
		Assert.Same(ex, exception.InnerException);
	}

	[Fact]
	public async Task HandleUpdateVacancies__Should_UpdateAllMessagesForEachVacancy()
	{
		// Arrange
		var vacancy1 = new Vacancy { Id = 1 };
		var vacancy2 = new Vacancy { Id = 2 };
		var vacancies = new List<Vacancy> { vacancy1, vacancy2 };
		var messages = new Dictionary<long, List<VacancyMessage>>
		{
			[1] = [new VacancyMessage(), new VacancyMessage()],
			[2] = [new VacancyMessage()]
		};

		_repositoryMock.Setup(x => x.GetMessagesByVacancies(vacancies, It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => messages);

		// Act
		await _sut.HandleUpdateVacancies(vacancies, CancellationToken.None);

		// Assert
		_botMock.Verify(
			x => x.UpdateMessageWithVacancy(vacancy1, It.IsAny<VacancyMessage>(), It.IsAny<CancellationToken>()),
			Times.Exactly(messages[1].Count));
		_botMock.Verify(
			x => x.UpdateMessageWithVacancy(vacancy2, It.IsAny<VacancyMessage>(), It.IsAny<CancellationToken>()),
			Times.Exactly(messages[2].Count));
	}


	[Fact]
	public async Task InitNewChat__Should_SendAllVacanciesToNewChat_And_SaveMessages()
	{
		// Arrange
		const long chatId = 1;
		var vacancies = new List<Vacancy>
		{
			new() { Id = 1 },
			new() { Id = 2 }
		};

		_vacanciesApiMock.Setup(x => x.GetVacancies(It.IsAny<CancellationToken>()))
			.ReturnsAsync(vacancies);

		_botMock.Setup(x => x.SendNewVacancyMessage(It.IsAny<Vacancy>(), chatId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => 1000);

		// Act
		await _sut.InitNewChat(chatId, CancellationToken.None);

		// Assert
		_vacanciesApiMock.Verify(x =>
			x.GetVacancies(It.IsAny<CancellationToken>()), 
			Times.Once);
		
		_botMock.Verify(x => 
				x.SendNewVacancyMessage(It.IsAny<Vacancy>(), chatId, It.IsAny<CancellationToken>()), 
			Times.Exactly(vacancies.Count));
		
		_repositoryMock.Verify(x => 
			x.AddMessages(
				It.Is<List<VacancyMessage>>(messages => messages.Count == vacancies.Count), 
				It.IsAny<CancellationToken>()),
			Times.Once);
	}


	[Fact]
	public async Task RequestVacancyInfoFill__Should_CallApiWithCorrectVacancyId()
	{
		// Arrange
		const int vacancyId = 1000;

		// Act
		await _sut.RequestVacancyInfoFill(vacancyId, CancellationToken.None);

		// Assert
		_vacanciesApiMock.Verify(x => 
				x.RequestVacancyInfoFill(vacancyId, It.IsAny<CancellationToken>()), 
			Times.Once);
	}

	[Fact]
	public async Task RequestVacancyInfoFill__Should_WrapException_WhenApiFails()
	{
		// Arrange
		var ex = new Exception("API error");
		_vacanciesApiMock.Setup(x => 
				x.RequestVacancyInfoFill(It.IsAny<long>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(ex);

		// Act
		var exception = await Record.ExceptionAsync(() => _sut.RequestVacancyInfoFill(1L, CancellationToken.None));
		
		// Assert
		Assert.NotNull(exception);
		Assert.Same(ex, exception.InnerException);
	}
}