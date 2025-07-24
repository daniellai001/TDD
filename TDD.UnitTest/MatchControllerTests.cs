using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using WebApplication1.Models;

namespace TDD.UnitTest;

public class MatchControllerTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task UpdateMatchResult_WhenNewMatch_ShouldReturn_ZeroZeroFirstHalf()
    {
        // Arrange - 測試初始狀態 (無記錄的比賽)
        var request = new UpdateMatchResultRequest
        {
            MatchId = 1,
            MatchEvent = MatchEvent.HomeGoal  // 任何事件都應該先建立比賽
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Match/UpdateMatchResult", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateMatchResultResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.DisplayResult, Is.EqualTo("1:0 (First Half)"));
    }

    [Test]
    public async Task UpdateMatchResult_WhenHomeGoal_ShouldReturn_OneZeroFirstHalf()
    {
        // Arrange
        var request = new UpdateMatchResultRequest
        {
            MatchId = 2,
            MatchEvent = MatchEvent.HomeGoal
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Match/UpdateMatchResult", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateMatchResultResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.DisplayResult, Is.EqualTo("1:0 (First Half)"));
    }

    [Test]
    public async Task UpdateMatchResult_WhenAwayGoal_ShouldReturn_ZeroOneFirstHalf()
    {
        // Arrange
        var request = new UpdateMatchResultRequest
        {
            MatchId = 3,
            MatchEvent = MatchEvent.AwayGoal
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Match/UpdateMatchResult", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateMatchResultResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.DisplayResult, Is.EqualTo("0:1 (First Half)"));
    }

    [Test]
    public async Task UpdateMatchResult_CompleteFlow_HomeGoalThenAwayGoalThenNextPeriod()
    {
        // Arrange & Act - 完整流程測試：主隊進球 -> 客隊進球 -> 換場
        var matchId = 4;

        // 主隊進球
        var homeGoalRequest = new UpdateMatchResultRequest { MatchId = matchId, MatchEvent = MatchEvent.HomeGoal };
        var json1 = JsonSerializer.Serialize(homeGoalRequest);
        var content1 = new StringContent(json1, Encoding.UTF8, "application/json");
        var response1 = await _client.PostAsync("/api/Match/UpdateMatchResult", content1);
        response1.EnsureSuccessStatusCode();

        // 客隊進球  
        var awayGoalRequest = new UpdateMatchResultRequest { MatchId = matchId, MatchEvent = MatchEvent.AwayGoal };
        var json2 = JsonSerializer.Serialize(awayGoalRequest);
        var content2 = new StringContent(json2, Encoding.UTF8, "application/json");
        var response2 = await _client.PostAsync("/api/Match/UpdateMatchResult", content2);
        response2.EnsureSuccessStatusCode();

        // 換場
        var nextPeriodRequest = new UpdateMatchResultRequest { MatchId = matchId, MatchEvent = MatchEvent.NextPeriod };
        var json3 = JsonSerializer.Serialize(nextPeriodRequest);
        var content3 = new StringContent(json3, Encoding.UTF8, "application/json");
        var response3 = await _client.PostAsync("/api/Match/UpdateMatchResult", content3);

        // Assert
        response3.EnsureSuccessStatusCode();
        var responseBody = await response3.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateMatchResultResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.DisplayResult, Is.EqualTo("1:1 (Second Half)"));
    }

    [Test]
    public async Task UpdateMatchResult_AwayCancel_InSecondHalf()
    {
        // Arrange & Act - 下半場取消客隊進球
        var matchId = 5;

        // 建立初始狀態：主隊進球 -> 客隊進球 -> 換場 (MatchResult = "HA;")
        await PostMatchEvent(matchId, MatchEvent.HomeGoal);
        await PostMatchEvent(matchId, MatchEvent.AwayGoal);
        await PostMatchEvent(matchId, MatchEvent.NextPeriod);

        // 取消客隊進球
        var cancelRequest = new UpdateMatchResultRequest { MatchId = matchId, MatchEvent = MatchEvent.AwayCancel };
        var json = JsonSerializer.Serialize(cancelRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/Match/UpdateMatchResult", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateMatchResultResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.DisplayResult, Is.EqualTo("1:0 (Second Half)"));
    }

    [Test]
    public async Task UpdateMatchResult_HomeCancel_InFirstHalf()
    {
        // Arrange & Act - 上半場取消主隊進球
        var matchId = 6;

        // 建立初始狀態：主隊進球 -> 客隊進球 -> 主隊進球 (最後字符是 'H')
        await PostMatchEvent(matchId, MatchEvent.HomeGoal);
        await PostMatchEvent(matchId, MatchEvent.AwayGoal);
        await PostMatchEvent(matchId, MatchEvent.HomeGoal);  // "HAH" - 最後字符是 'H'

        // 取消主隊進球
        var cancelRequest = new UpdateMatchResultRequest { MatchId = matchId, MatchEvent = MatchEvent.HomeCancel };
        var json = JsonSerializer.Serialize(cancelRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/Match/UpdateMatchResult", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UpdateMatchResultResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.That(result, Is.Not.Null);
        Assert.That(result.DisplayResult, Is.EqualTo("1:1 (First Half)"));
    }

    [Test]
    public async Task UpdateMatchResult_CannotCancelAwayWhenNoAwayGoalsExist_ShouldReturn500()
    {
        // Arrange - 建立只有主隊進球的錯誤場景: "HH;H" (無客隊進球)
        var matchId = 7;
        await PostMatchEvent(matchId, MatchEvent.HomeGoal);   // "H"
        await PostMatchEvent(matchId, MatchEvent.HomeGoal);   // "HH"
        await PostMatchEvent(matchId, MatchEvent.NextPeriod); // "HH;"
        await PostMatchEvent(matchId, MatchEvent.HomeGoal);   // "HH;H"

        // Act - 嘗試取消不存在的客隊進球
        var cancelRequest = new UpdateMatchResultRequest { MatchId = matchId, MatchEvent = MatchEvent.AwayCancel };
        var json = JsonSerializer.Serialize(cancelRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/Match/UpdateMatchResult", content);

        // Assert - 應該回傳錯誤狀態碼
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task UpdateMatchResult_CannotCancelHomeWhenNoHomeGoalsExist_ShouldReturn500()
    {
        // Arrange - 建立只有客隊進球的錯誤場景: "AA;A" (無主隊進球)
        var matchId = 8;
        await PostMatchEvent(matchId, MatchEvent.AwayGoal);   // "A"
        await PostMatchEvent(matchId, MatchEvent.AwayGoal);   // "AA"
        await PostMatchEvent(matchId, MatchEvent.NextPeriod); // "AA;"
        await PostMatchEvent(matchId, MatchEvent.AwayGoal);   // "AA;A"

        // Act - 嘗試取消不存在的主隊進球
        var cancelRequest = new UpdateMatchResultRequest { MatchId = matchId, MatchEvent = MatchEvent.HomeCancel };
        var json = JsonSerializer.Serialize(cancelRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/Match/UpdateMatchResult", content);

        // Assert - 應該回傳錯誤狀態碼
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
    }

    // Helper method to simplify event posting
    private async Task PostMatchEvent(int matchId, MatchEvent matchEvent)
    {
        var request = new UpdateMatchResultRequest { MatchId = matchId, MatchEvent = matchEvent };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/Match/UpdateMatchResult", content);
        response.EnsureSuccessStatusCode();
    }
} 