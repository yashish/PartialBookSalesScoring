using PartialBookSalesScoring.Models;
using Microsoft.Extensions.Options;

namespace PartialBookSalesScoring.Services  
{
    public class ScoringService : IScoringService
    {
        private readonly ScoringOptions _opts;
        public ScoringService(IOptions<ScoringOptions> opts)
        {
            _opts = opts.Value;
        }

        public ScoreResult Score(Account account)
        {
            // Compute each sub-score on 0..100 scale (examples)
            var profitability = ComputeProfitabilityScore(account);
            var servicing = ComputeServicingScore(account);
            var strategic = ComputeStrategicFitScore(account);
            var compliance = ComputeComplianceScore(account);
            var buyerInterest = ComputeBuyerInterestScore(account);

            // Weighted combination
            double rawScore =
                (_opts.ProfitabilityWeight * profitability) +
                (_opts.ServicingWeight * servicing) +
                (_opts.StrategicFitWeight * strategic) +
                (_opts.ComplianceWeight * compliance) +
                (_opts.BuyerInterestWeight * buyerInterest);

            int finalScore = (int)Math.Round(rawScore);

            var breakdown = new ScoreBreakdown
            {
                ProfitabilityScore = profitability,
                ServicingScore = servicing,
                StrategicFitScore = strategic,
                ComplianceScore = compliance,
                BuyerInterestScore = buyerInterest
            };

            var label = buyerInterest switch
            {
                >= 70 => "High",
                >= 40 => "Medium",
                _ => "Low"
            };

            // Simple explanation text
            var explanation = Explain(account, breakdown, finalScore);

            return new ScoreResult
            {
                AccountId = account.AccountId,
                Score = Math.Clamp(finalScore, 0, 100),
                Breakdown = breakdown,
                BuyerInterestLabel = label,
                Explanation = explanation
            };
        }

        public IEnumerable<ScoreResult> Rank(IEnumerable<Account> accounts, int topN = 0)
        {
            var results = accounts.Select(Score).OrderByDescending(r => r.Score).ToList();
            if (topN > 0) return results.Take(topN);
            return results;
        }

        #region Scoring Helpers (simple heuristics)

        private double ComputeProfitabilityScore(Account a)
        {
            // Basic heuristics:
            // higher AUM and higher revenue => higher profitability.
            // feeType 'advisory' tends to be higher margin than 'commission'
            decimal aumScore = Normalize(a.AUM, 0m, 2_000_000m); // cap at 2M
            decimal revenueScore = Normalize(a.AnnualRevenue, 0m, 100_000m); // cap
            decimal feeBoost = a.FeeType?.ToLower() == "advisory" ? 1.1m : 0.9m;

            var combined = (0.6m * aumScore + 0.4m * revenueScore) * feeBoost;
            return (double)ScaleTo100(combined);
        }

        private double ComputeServicingScore(Account a)
        {
            // High servicing burden -> higher servicing score (we treat as bad)
            // We want large servicing burden to increase the "sell" signal; to keep semantics consistent we'll invert:
            // more tickets/meetings -> higher servicingScore (i.e., a reason to sell)
            decimal tickets = Normalize(a.ServiceTicketsPerYear, 0m, 50m);
            decimal meetings = Normalize(a.MeetingsPerYear, 0m, 24m);
            decimal complexity = a.HasComplexHoldings ? 1.0m : 0.0m;

            var val = (0.5m * tickets + 0.4m * meetings + 0.1m * complexity);
            return (double)ScaleTo100(val);
        }

        private double ComputeStrategicFitScore(Account a)
        {
            // Lower fit => higher score (we recommend selling accounts that don't fit)
            // For this example assume target is HNW > $500k and age 40-65
            decimal aumFit = a.AUM >= 500_000m ? 0.0m : 1.0m; // if below target, it's more likely to be sold
            decimal ageFit = (a.Age >= 40 && a.Age <= 65) ? 0.0m : 1.0m;
            // region mismatch could be added. Combine:
            var val = (0.7m * aumFit + 0.3m * ageFit);
            return (double)ScaleTo100(val);
        }

        private double ComputeComplianceScore(Account a)
        {
            // Simpler: complex holdings contribute to compliance risk
            decimal val = a.HasComplexHoldings ? 1.0m : 0.0m;
            return (double)ScaleTo100(val);
        }

        private double ComputeBuyerInterestScore(Account a)
        {
            // Heuristic buyer interest: buyers prefer larger AUM and simple holdings
            decimal aumPref = Normalize(a.AUM, 0m, 1_000_000m); // scaled
            decimal complexityPenalty = a.HasComplexHoldings ? 0.0m : 1.0m;
            var val = 0.8m * aumPref + 0.2m * complexityPenalty;
            return (double)ScaleTo100(val);
        }

        #endregion

        #region Utility methods

        private static decimal Normalize(decimal value, decimal min, decimal max)
        {
            if (max <= min) return 0m;
            var v = (value - min) / (max - min);
            if (v < 0m) return 0m;
            if (v > 1m) return 1m;
            return v;
        }

        // Input expected 0..1 combineish -> scale to 0..100 range
        private static decimal ScaleTo100(decimal x)
        {
            var r = x * 100m;
            if (r < 0m) return 0m;
            if (r > 100m) return 100m;
            return r;
        }

        private string Explain(Account a, ScoreBreakdown b, int finalScore)
        {
            var reasons = new List<string>();
            if (b.ProfitabilityScore < 30)
                reasons.Add("Low profitability (low AUM / revenue).");
            else if (b.ProfitabilityScore >= 70)
                reasons.Add("High profitability — consider keeping unless servicing burden is high.");

            if (b.ServicingScore >= 50)
                reasons.Add("High servicing requirements (tickets/meetings/complex holdings).");

            if (b.StrategicFitScore >= 50)
                reasons.Add("Low strategic fit with advisor target client profile.");

            if (b.ComplianceScore >= 50)
                reasons.Add("Complex holdings may cause compliance overhead.");

            if (b.BuyerInterestScore >= 70)
                reasons.Add("Strong buyer interest predicted.");

            if (!reasons.Any())
                reasons.Add("No major reasons flagged; keep account under current servicing.");

            return $"Score {finalScore}: " + string.Join(" ", reasons);
        }

        #endregion
    }
}
