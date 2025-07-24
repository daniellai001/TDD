using WebApplication1.Models;
using WebApplication1.Repositories;
using WebApplication1.Services;

namespace TDD.UnitTest;

public class MatchServiceTests
{
    private IMatchService _service;
    private IMatchRepository _repository;

    [SetUp]
    public void Setup()
    {
        _repository = new MatchRepository();
        _service = new MatchService(_repository);
    }

    [Test]
    public async Task UpdateMatchResultAsync_WhenHomeGoal_ShouldReturnMatchResultH()
    {
        // Arrange
        int matchId = 1;
        var matchEvent = MatchEvent.HomeGoal;

        // Act
        var result = await _service.UpdateMatchResultAsync(matchId, matchEvent);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchId, Is.EqualTo(matchId));
        Assert.That(result.MatchResult, Is.EqualTo("H"));
    }

    [Test]
    public async Task UpdateMatchResultAsync_WhenAwayGoal_ShouldReturnMatchResultA()
    {
        // Arrange
        int matchId = 1;
        var matchEvent = MatchEvent.AwayGoal;

        // Act
        var result = await _service.UpdateMatchResultAsync(matchId, matchEvent);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchId, Is.EqualTo(matchId));
        Assert.That(result.MatchResult, Is.EqualTo("A"));
    }

    [Test]
    public async Task UpdateMatchResultAsync_WhenNextPeriod_ShouldAddSemicolon()
    {
        // Arrange
        int matchId = 1;
        
        // 先添加一些進球事件
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayGoal);

        // Act - 換場
        var result = await _service.UpdateMatchResultAsync(matchId, MatchEvent.NextPeriod);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchResult, Is.EqualTo("HA;"));
        Assert.That(result.GetDisplayResult(), Is.EqualTo("1:1 (Second Half)"));
    }

    [Test]
    public async Task UpdateMatchResultAsync_WhenNextPeriodCalledTwice_ShouldNotAddExtraSemicolon()
    {
        // Arrange
        int matchId = 1;
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.NextPeriod);

        // Act - 再次呼叫 NextPeriod
        var result = await _service.UpdateMatchResultAsync(matchId, MatchEvent.NextPeriod);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchResult, Is.EqualTo("H;")); // 不應該有額外的分號
    }

    [Test]
    public async Task UpdateMatchResultAsync_WhenHomeCancel_ShouldRemoveLastH()
    {
        // Arrange
        int matchId = 1;
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayGoal);
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);
        // 目前狀態: "HAH" - 去除分號後最後字符是 'H'

        // Act - 取消主隊進球
        var result = await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeCancel);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchResult, Is.EqualTo("HA"));
        Assert.That(result.GetDisplayResult(), Is.EqualTo("1:1 (First Half)"));
    }

    [Test]
    public async Task UpdateMatchResultAsync_WhenAwayCancel_ShouldRemoveLastA()
    {
        // Arrange  
        int matchId = 1;
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayGoal);
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.NextPeriod);
        // 目前狀態: "HA;" - 去除分號後 "HA"，最後字符是 'A'

        // Act - 取消客隊進球
        var result = await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayCancel);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchResult, Is.EqualTo("H;"));
        Assert.That(result.GetDisplayResult(), Is.EqualTo("1:0 (Second Half)"));
    }

    [Test]
    public async Task UpdateMatchResultAsync_WhenHomeCancelWithEmptyResult_ShouldThrowException()
    {
        // Arrange
        int matchId = 1;
        // 目前狀態: "" - 空字串

        // Act & Assert - 試圖取消空字串應該拋出例外
        try
        {
            await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeCancel);
            Assert.Fail("應該拋出 UpdateMatchResultException");
        }
        catch (UpdateMatchResultException ex)
        {
            Assert.That(ex.MatchId, Is.EqualTo(matchId));
            Assert.That(ex.MatchEvent, Is.EqualTo(MatchEvent.HomeCancel));
            Assert.That(ex.MatchResult, Is.EqualTo(""));
            Assert.That(ex.Message, Is.EqualTo("1, HomeCancel + "));
        }
    }

    [Test]
    public async Task UpdateMatchResultAsync_WhenAwayCancelWithEmptyResult_ShouldThrowException()
    {
        // Arrange
        int matchId = 1;
        // 目前狀態: "" - 空字串

        // Act & Assert - 試圖取消空字串應該拋出例外
        try
        {
            await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayCancel);
            Assert.Fail("應該拋出 UpdateMatchResultException");
        }
        catch (UpdateMatchResultException ex)
        {
            Assert.That(ex.MatchId, Is.EqualTo(matchId));
            Assert.That(ex.MatchEvent, Is.EqualTo(MatchEvent.AwayCancel));
            Assert.That(ex.MatchResult, Is.EqualTo(""));
            Assert.That(ex.Message, Is.EqualTo("1, AwayCancel + "));
        }
    }

    [Test]
    public async Task UpdateMatchResultAsync_CannotCancelAwayWhenLastCharacterIsNotA_ShouldThrowException()
    {
        // Arrange - AC 指定案例: MatchResult = "HHA;H"
        int matchId = 1;
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);   // "H"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);   // "HH"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayGoal);   // "HHA"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.NextPeriod); // "HHA;"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);   // "HHA;H"
        // 去除分號後 "HHAH"，最後字符是 'H'，不是 'A'

        // Act & Assert - 嘗試取消客隊進球但最後字符是 'H'
        try
        {
            await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayCancel);
            Assert.Fail("應該拋出 UpdateMatchResultException");
        }
        catch (UpdateMatchResultException ex)
        {
            Assert.That(ex.MatchId, Is.EqualTo(matchId));
            Assert.That(ex.MatchEvent, Is.EqualTo(MatchEvent.AwayCancel));
            Assert.That(ex.MatchResult, Is.EqualTo("HHA;H"));
            Assert.That(ex.Message, Is.EqualTo("1, AwayCancel + HHA;H"));
        }
    }

    [Test]
    public async Task UpdateMatchResultAsync_CannotCancelHomeWhenLastCharacterIsNotH_ShouldThrowException()
    {
        // Arrange - AC 指定案例: MatchResult = "AHA;A"
        int matchId = 1;
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayGoal);   // "A"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);   // "AH"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayGoal);   // "AHA"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.NextPeriod); // "AHA;"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayGoal);   // "AHA;A"
        // 去除分號後 "AHAA"，最後字符是 'A'，不是 'H'

        // Act & Assert - 嘗試取消主隊進球但最後字符是 'A'
        try
        {
            await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeCancel);
            Assert.Fail("應該拋出 UpdateMatchResultException");
        }
        catch (UpdateMatchResultException ex)
        {
            Assert.That(ex.MatchId, Is.EqualTo(matchId));
            Assert.That(ex.MatchEvent, Is.EqualTo(MatchEvent.HomeCancel));
            Assert.That(ex.MatchResult, Is.EqualTo("AHA;A"));
            Assert.That(ex.Message, Is.EqualTo("1, HomeCancel + AHA;A"));
        }
    }

    [Test]
    public async Task UpdateMatchResultAsync_CannotCancelAwayWhenOnlyH_ShouldThrowException()
    {
        // Arrange - 只有主隊進球: "H;"
        int matchId = 1;
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);   // "H"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.NextPeriod); // "H;"
        // 去除分號後 "H"，最後字符是 'H'，不是 'A'

        // Act & Assert - 嘗試取消客隊進球
        try
        {
            await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayCancel);
            Assert.Fail("應該拋出 UpdateMatchResultException");
        }
        catch (UpdateMatchResultException ex)
        {
            Assert.That(ex.MatchId, Is.EqualTo(matchId));
            Assert.That(ex.MatchEvent, Is.EqualTo(MatchEvent.AwayCancel));
            Assert.That(ex.MatchResult, Is.EqualTo("H;"));
            Assert.That(ex.Message, Is.EqualTo("1, AwayCancel + H;"));
        }
    }

    [Test]
    public async Task UpdateMatchResultAsync_ComplexScenario_ShouldHandleCorrectly()
    {
        // Arrange & Act - 複雜場景測試
        int matchId = 1;
        
        // 建立複雜的比賽狀態: H -> A -> H -> ; -> A -> 取消A -> H
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);      // "H"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayGoal);      // "HA"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);      // "HAH" 
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.NextPeriod);    // "HAH;"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayGoal);      // "HAH;A"
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.AwayCancel);    // "HAH;" (去除分號後 "HAHA"，最後是 'A'，可以取消)
        var result = await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal); // "HAH;H"

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchResult, Is.EqualTo("HAH;H"));
        Assert.That(result.GetDisplayResult(), Is.EqualTo("3:1 (Second Half)"));
    }

    [Test]
    public async Task GetMatchAsync_WhenMatchNotExists_ShouldReturnNull()
    {
        // Arrange
        int matchId = 999;

        // Act
        var result = await _service.GetMatchAsync(matchId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetMatchAsync_WhenMatchExists_ShouldReturnMatch()
    {
        // Arrange
        int matchId = 1;
        await _service.UpdateMatchResultAsync(matchId, MatchEvent.HomeGoal);

        // Act
        var result = await _service.GetMatchAsync(matchId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.MatchId, Is.EqualTo(matchId));
        Assert.That(result.MatchResult, Is.EqualTo("H"));
    }
} 