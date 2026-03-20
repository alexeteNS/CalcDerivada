using Models;

namespace Interfaces
{
    public interface IDerivationRepository
    {
        DerivationOutput PowerRule(Polynomial poly);
        DerivationOutput NthDerivative(Polynomial poly, int n);
        DerivationOutput ProductRule(Polynomial f, Polynomial g);
        DerivationOutput QuotientRule(Polynomial f, Polynomial g);
        DerivationOutput Integral(Polynomial poly);
        DerivationOutput DefiniteIntegral(Polynomial poly, double a, double b);
        DerivationOutput CriticalPoints(Polynomial poly);
        DerivationOutput ConcavityAnalysis(Polynomial poly);
        DerivationOutput RolleTheorem(Polynomial poly, double a, double b);
        DerivationOutput EvaluatePoint(Polynomial poly, double x);
    }
}
