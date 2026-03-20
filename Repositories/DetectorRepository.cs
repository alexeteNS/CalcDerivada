using Interfaces;
using Models;

namespace Repositories
{
    public class DetectorRepository : IDetectorRepository
    {
        public DerivationType DetectType(DerivationInput input)
        {
            string raw = input.Raw.Trim();

            if (input.NthOrder > 1)
                return DerivationType.NthDerivative;

            if (!string.IsNullOrWhiteSpace(input.RawSecond))
                return input.RawSecond!.TrimStart().StartsWith("/")
                    ? DerivationType.QuotientRule
                    : DerivationType.ProductRule;

            if (raw.StartsWith("R") && input.A.HasValue && input.B.HasValue)
                return DerivationType.RolleTheorem;

            if (raw.StartsWith("I") && input.A.HasValue && input.B.HasValue)
                return DerivationType.DefiniteIntegral;

            if (raw.StartsWith("I"))
                return DerivationType.Integral;

            if (raw.StartsWith("C"))
                return DerivationType.CriticalPoints;

            if (raw.StartsWith("K"))
                return DerivationType.ConcavityAnalysis;

            if (input.A.HasValue && !input.B.HasValue)
                return DerivationType.EvaluatePoint;

            return DerivationType.PowerRule;
        }
    }
}
