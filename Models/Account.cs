namespace PartialBookSalesScoring.Models
{
    // DTO representing an account used as input to the scoring engine
    public class Account
    {
        public string AccountId { get; set; } = string.Empty;
        public string AdvisorId { get; set; } = string.Empty;
        public decimal AUM { get; set; }                        // Assets under management
        public decimal AnnualRevenue { get; set; }
        public string FeeType { get; set; } = "advisory";       // advisory | commission
        public int MeetingsPerYear { get; set; }
        public int ServiceTicketsPerYear { get; set; }
        public int Age { get; set; }                            // client age
        public bool HasComplexHoldings { get; set; } = false;   // estates, trusts, etc
        public string Region { get; set; } = "unknown";
        // Add more fields as needed...
    }
}
