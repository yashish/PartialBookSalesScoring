namespace PartialBookSalesScoring.Models
{
    public class ScoreBreakdown
    {
        public double ProfitabilityScore { get; set; }
        public double ServicingScore { get; set; }
        public double StrategicFitScore { get; set; }
        public double ComplianceScore { get; set; }
        public double BuyerInterestScore { get; set; }
    }

    public class ScoreResult
    {
        public string AccountId { get; set; } = string.Empty;
        public int Score { get; set; } // 0-100
        public ScoreBreakdown Breakdown { get; set; } = new ScoreBreakdown();
        public string BuyerInterestLabel { get; set; } = "Unknown"; // Low / Medium / High
        public string Explanation { get; set; } = string.Empty;
    }
}
