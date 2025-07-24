using WebApplication1.Models;
using WebApplication1.Repositories;

namespace TDD.UnitTest;

public class MatchRepositoryTests
{
    private IMatchRepository _repository;

    [SetUp]
    public void Setup()
    {
        _repository = new MatchRepository();
    }

    [Test]
    public async Task GetMatchAsync_WhenMatchNotExists_ShouldReturnNull()
    {
        // Arrange
        int matchId = 999;

        // Act
        var result = await _repository.GetMatchAsync(matchId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SaveMatchAsync_WhenNewMatch_ShouldStoreAndReturnMatch()
    {
        // Arrange
        var match = new Match { MatchId = 1, MatchResult = "H" };

        // Act
        var result = await _repository.SaveMatchAsync(match);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchId, Is.EqualTo(1));
        Assert.That(result.MatchResult, Is.EqualTo("H"));
    }

    [Test]
    public async Task SaveMatchAsync_WhenExistingMatch_ShouldUpdateAndReturnMatch()
    {
        // Arrange
        var match = new Match { MatchId = 1, MatchResult = "H" };
        await _repository.SaveMatchAsync(match);

        var updatedMatch = new Match { MatchId = 1, MatchResult = "HA" };

        // Act
        var result = await _repository.SaveMatchAsync(updatedMatch);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchId, Is.EqualTo(1));
        Assert.That(result.MatchResult, Is.EqualTo("HA"));
    }

    [Test]
    public async Task GetMatchAsync_WhenMatchExists_ShouldReturnMatch()
    {
        // Arrange
        var match = new Match { MatchId = 1, MatchResult = "H" };
        await _repository.SaveMatchAsync(match);

        // Act
        var result = await _repository.GetMatchAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchId, Is.EqualTo(1));
        Assert.That(result.MatchResult, Is.EqualTo("H"));
    }

    [Test]
    public async Task SaveMatchAsync_WhenMultipleMatches_ShouldMaintainSeparateStates()
    {
        // Arrange
        var match1 = new Match { MatchId = 1, MatchResult = "H" };
        var match2 = new Match { MatchId = 2, MatchResult = "A" };

        // Act
        await _repository.SaveMatchAsync(match1);
        await _repository.SaveMatchAsync(match2);

        // Assert
        var result1 = await _repository.GetMatchAsync(1);
        var result2 = await _repository.GetMatchAsync(2);

        Assert.That(result1, Is.Not.Null);
        Assert.That(result1.MatchResult, Is.EqualTo("H"));
        Assert.That(result2, Is.Not.Null);
        Assert.That(result2.MatchResult, Is.EqualTo("A"));
    }
} 