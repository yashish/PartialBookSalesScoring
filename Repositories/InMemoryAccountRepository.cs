using PartialBookSalesScoring.Models;

namespace PartialBookSalesScoring.Repositories
{
    public class InMemoryAccountRepository : IAccountRepository
    {
        private readonly List<Account> _store = new()
        {
            new Account { AccountId = "A1", AdvisorId = "ADV1", AUM = 120000, AnnualRevenue = 1800, FeeType = "advisory", MeetingsPerYear = 2, ServiceTicketsPerYear = 1, Age = 35, HasComplexHoldings = false, Region = "NE" },
            new Account { AccountId = "A2", AdvisorId = "ADV1", AUM = 450000, AnnualRevenue = 6000, FeeType = "advisory", MeetingsPerYear = 6, ServiceTicketsPerYear = 10, Age = 72, HasComplexHoldings = true, Region = "CA" },
            new Account { AccountId = "A3", AdvisorId = "ADV1", AUM = 25000, AnnualRevenue = 200, FeeType = "commission", MeetingsPerYear = 1, ServiceTicketsPerYear = 0, Age = 28, HasComplexHoldings = false, Region = "TX" }
        };

        public Task<Account?> GetByIdAsync(string accountId)
        {
            return Task.FromResult(_store.FirstOrDefault(a => a.AccountId == accountId));
        }

        public Task<IEnumerable<Account>> GetByAdvisorAsync(string advisorId, int limit = 100)
        {
            return Task.FromResult(_store.Where(a => a.AdvisorId == advisorId).Take(limit));
        }

        public Task SaveScoreAsync(string accountId, int score)
        {
            // For MVP we simply log or ignore; in production write to DynamoDB / Redshift
            Console.WriteLine($"[InMemoryRepo] Save score {score} for {accountId}");
            return Task.CompletedTask;
        }
    }
}
