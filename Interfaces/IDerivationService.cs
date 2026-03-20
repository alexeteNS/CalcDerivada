using Models;

namespace Interfaces
{
    public interface IDerivationService
    {
        DerivationOutput Solve(ParsedDerivation parsed);
    }
}
