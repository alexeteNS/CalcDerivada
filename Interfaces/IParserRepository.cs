using Models;

namespace Interfaces
{
    public interface IParserRepository
    {
        Polynomial      ParsePolynomial(string raw);
        ParsedDerivation BuildParsed(DerivationInput input, DerivationType type);
    }
}
