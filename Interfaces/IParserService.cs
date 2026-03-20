using Models;

namespace Interfaces
{
    public interface IParserService
    {
        ParsedDerivation Parse(DerivationInput input, DerivationType type);
    }
}
