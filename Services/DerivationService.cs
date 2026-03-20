using Interfaces;
using Models;

namespace Services
{
    public class DerivationService : IDerivationService
    {
        private readonly IDerivationRepository _repo;

        public DerivationService(IDerivationRepository repo) => _repo = repo;

        public DerivationOutput Solve(ParsedDerivation parsed)
        {
            return parsed.Type switch
            {
                DerivationType.PowerRule         => _repo.PowerRule(parsed.Polynomial),
                DerivationType.NthDerivative     => _repo.NthDerivative(parsed.Polynomial, parsed.NthOrder),
                DerivationType.ProductRule       => _repo.ProductRule(parsed.Polynomial, parsed.Second!),
                DerivationType.QuotientRule      => _repo.QuotientRule(parsed.Polynomial, parsed.Second!),
                DerivationType.Integral          => _repo.Integral(parsed.Polynomial),
                DerivationType.DefiniteIntegral  => _repo.DefiniteIntegral(parsed.Polynomial, parsed.A!.Value, parsed.B!.Value),
                DerivationType.CriticalPoints    => _repo.CriticalPoints(parsed.Polynomial),
                DerivationType.ConcavityAnalysis => _repo.ConcavityAnalysis(parsed.Polynomial),
                DerivationType.RolleTheorem      => _repo.RolleTheorem(parsed.Polynomial, parsed.A!.Value, parsed.B!.Value),
                DerivationType.EvaluatePoint     => _repo.EvaluatePoint(parsed.Polynomial, parsed.A!.Value),
                _                                => new DerivationOutput { Success = false, Description = "Tipo desconocido" }
            };
        }
    }
}
