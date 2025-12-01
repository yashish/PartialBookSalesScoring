namespace PartialBookSalesScoring.Services
{
    public class ScoringOptions
    {
        public double ProfitabilityWeight { get; set; } = 0.3;
        public double ServicingWeight { get; set; } = 0.25;
        public double StrategicFitWeight { get; set; } = 0.2;
        public double ComplianceWeight { get; set; } = 0.1;
        public double BuyerInterestWeight { get; set; } = 0.15;

        public int RecommendationThreshold { get; set; } = 80;
        public int RecommendCount { get; set; } = 5;
    }
}
