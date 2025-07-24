using Accounting.Models;
using Accounting.Repo;

namespace Accounting.Services;

public class BudgetService(IBudgetRepo budgetRepo)
{
    public decimal Query(Period period)
    {
        if (period.IsValid())
        {
            return 0m;
        }
        var budgets = budgetRepo.GetAll();
        var totalAmount = 0m;
        var currentDate = period.From;

        while (currentDate <= period.To)
        {
            var yearMonth = currentDate.ToString("yyyyMM");
            var budget = budgets.FirstOrDefault(x => x.YearMonth == yearMonth);
            
            if (budget != null)
            {
                var monthDays = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                var dailyAmount = (decimal)budget.Amount / monthDays;
                totalAmount += dailyAmount;
            }
            
            currentDate = currentDate.AddDays(1);
        }

        return totalAmount;
    }
}