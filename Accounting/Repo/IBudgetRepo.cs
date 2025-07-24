using Accounting.Domains;

namespace Accounting.Repo;

public interface IBudgetRepo
{
    List<Budget> GetAll();
}