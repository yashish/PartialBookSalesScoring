using PartialBookSalesScoring.Models;

namespace PartialBookSalesScoring.Services
{
    public interface IScoringService
    {
        ScoreResult Score(Account account);
        IEnumerable<ScoreResult> Rank(IEnumerable<Account> accounts, int topN = 0);
    }
}
