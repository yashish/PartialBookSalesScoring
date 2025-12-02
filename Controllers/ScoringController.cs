using PartialBookSalesScoring.Models;
using PartialBookSalesScoring.Repositories;
using PartialBookSalesScoring.Services;
using Microsoft.AspNetCore.Mvc;

namespace PartialBookSalesScoring.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScoringController : ControllerBase
    {
        private readonly IScoringService _scoringService;
        private readonly IAccountRepository _accountRepo;
        private readonly ILogger<ScoringController> _logger;

        public ScoringController(IScoringService scoringService, IAccountRepository accountRepo, ILogger<ScoringController> logger)
        {
            _scoringService = scoringService;
            _accountRepo = accountRepo;
            _logger = logger;
        }

        // Score one account payload
        [HttpPost("score")]
        public IActionResult Score([FromBody] Account account)
        {
            if (account == null || string.IsNullOrEmpty(account.AccountId)) 
                return BadRequest("AccountId required");
            
            var result = _scoringService.Score(account);
            
            // optionally persist
            _accountRepo.SaveScoreAsync(account.AccountId, result.Score).ConfigureAwait(false);
            return Ok(result);
        }

        // Rank accounts by advisor
        [HttpPost("rank")]
        public async Task<IActionResult> Rank([FromQuery] string advisorId, [FromBody] List<Account>? accounts = null, [FromQuery] int top = 0)
        {
            if (string.IsNullOrEmpty(advisorId)) 
                return BadRequest("advisorId required");
            
            IEnumerable<Account> toRank;
            if (accounts != null && accounts.Count > 0)
            {
                toRank = accounts;
            }
            else
            {
                toRank = await _accountRepo.GetByAdvisorAsync(advisorId);
            }

            var ranked = _scoringService.Rank(toRank, top).ToList();
            // Optionally persist top N scores
            foreach (var r in ranked.Take(20))
            {
                await _accountRepo.SaveScoreAsync(r.AccountId, r.Score);
            }

            return Ok(ranked);
        }
    }
}
