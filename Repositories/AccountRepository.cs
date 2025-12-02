using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PartialBookSalesScoring.Models;

namespace PartialBookSalesScoring.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IAmazonDynamoDB _ddb;
        private readonly string _tableName;

        public AccountRepository(IAmazonDynamoDB ddb, IConfiguration config)
        {
            _ddb = ddb;
            _tableName = config.GetValue<string>("DynamoTableName") ?? "Accounts";
        }

        public async Task<Account?> GetByIdAsync(string accountId)
        {
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["AccountId"] = new AttributeValue { S = accountId }
                }
            };

            var resp = await _ddb.GetItemAsync(request);

            if (resp.Item == null || resp.Item.Count == 0) return null;
            return Map(resp.Item);
        }

        public async Task<IEnumerable<Account>> GetByAdvisorAsync(string advisorId, int limit = 100)
        {
            // This is a naive scan for demo. Use Query with GSI in production.
            var scan = await _ddb.ScanAsync(new ScanRequest
            {
                TableName = _tableName,
                FilterExpression = "AdvisorId = :adv",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { [":adv"] = new AttributeValue { S = advisorId } },
                Limit = limit
            });

            return scan.Items.Select(Map);
        }

        public async Task SaveScoreAsync(string accountId, int score)
        {
            // persist a score attribute — in production you'd use a separate table for score history
            var request = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue> { ["AccountId"] = new AttributeValue { S = accountId } },
                UpdateExpression = "SET LastScore = :s",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { [":s"] = new AttributeValue { N = score.ToString() } }
            };
            await _ddb.UpdateItemAsync(request);
        }

        private static Account Map(Dictionary<string, AttributeValue> item)
        {
            return new Account
            {
                AccountId = item.GetValueOrDefault("AccountId")?.S ?? string.Empty,
                AdvisorId = item.GetValueOrDefault("AdvisorId")?.S ?? string.Empty,
                AUM = decimal.TryParse(item.GetValueOrDefault("AUM")?.N, out var aum) ? aum : 0,
                AnnualRevenue = decimal.TryParse(item.GetValueOrDefault("AnnualRevenue")?.N, out var rev) ? rev : 0,
                FeeType = item.GetValueOrDefault("FeeType")?.S ?? "advisory",
                MeetingsPerYear = int.TryParse(item.GetValueOrDefault("MeetingsPerYear")?.N, out var m) ? m : 0,
                ServiceTicketsPerYear = int.TryParse(item.GetValueOrDefault("ServiceTicketsPerYear")?.N, out var t) ? t : 0,
                Age = int.TryParse(item.GetValueOrDefault("Age")?.N, out var age) ? age : 0,
                HasComplexHoldings = item.GetValueOrDefault("HasComplexHoldings")?.BOOL ?? false,
                Region = item.GetValueOrDefault("Region")?.S ?? "unknown"
            };
        }
    }
}
