using PartialBookSalesScoring.Models;

namespace PartialBookSalesScoring.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(string accountId);
        Task<IEnumerable<Account>> GetByAdvisorAsync(string advisorId, int limit = 100);
        Task SaveScoreAsync(string accountId, int score);
    }
}
