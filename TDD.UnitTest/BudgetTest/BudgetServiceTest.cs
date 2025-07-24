using Accounting.Domains;
using Accounting.Models;
using Accounting.Repo;
using Accounting.Services;
using FluentAssertions;
using NSubstitute;

namespace TDD.UnitTest.BudgetTest;

[TestFixture]
public class BudgetServiceTest
{
    private IBudgetRepo _budgetRepo;
    private BudgetService _budgetService;

    [SetUp]
    public void SetUp()
    {
        _budgetRepo = Substitute.For<IBudgetRepo>();
        _budgetService = new BudgetService(_budgetRepo);
    }
    
    [Test]
    public void get_budget_one_day()
    {
        GivenBudgets([
            new()
            {
                YearMonth = "202507",
                Amount = 31
            }
        ]);
        var totalAmount = _budgetService.Query(new Period(new DateTime(2025,7,24), new DateTime(2025,7,24)));
        totalAmount.Should().Be(1m);
    }

    private void GivenBudgets(List<Budget> budgets)
    {
        _budgetRepo.GetAll().Returns(budgets);
    }

    [Test]
    public void get_budget_whole_month()
    {
        GivenBudgets([
            new()
            {
                YearMonth = "202507",
                Amount = 31
            }
        ]);
        var totalAmount = _budgetService.Query(new Period(new DateTime(2025,7,1), new DateTime(2025,7,31)));
        totalAmount.Should().Be(31m);
    }

    [Test]
    public void get_budget_partial_month()
    {
        GivenBudgets([
            new()
            {
                YearMonth = "202507",
                Amount = 31
            }
        ]);
        var totalAmount = _budgetService.Query(new Period(new DateTime(2025,7,1), new DateTime(2025,7,15)));
        totalAmount.Should().Be(15m);
    }
    
    [Test]
    public void get_budget_cross_month()
    {
        GivenBudgets([
            new()
            {
                YearMonth = "202507",
                Amount = 31
            },
            new()
            {
                YearMonth = "202508",
                Amount = 310
            }
        ]);
        var totalAmount = _budgetService.Query(new Period(new DateTime(2025,7,31), new DateTime(2025,8,1)));
        totalAmount.Should().Be(11m);
    }
    
    [Test]
    public void get_budget_invalid_period_should_return_zero()
    {
        var totalAmount = _budgetService.Query(new Period(new DateTime(2025,8,31), new DateTime(2025,8,1)));
        totalAmount.Should().Be(0m);
        _budgetRepo.DidNotReceive().GetAll();
    }
}