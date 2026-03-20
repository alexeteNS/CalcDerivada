using AngouriMath;
using Interfaces;
using Models;

namespace Repositories
{
    // El parser ya NO construye Polynomials a mano.
    // Guarda la expresión como string en Polynomial.RawExpr
    // y AngouriMath la trabaja en DerivationRepository.
    public class ParserRepository : IParserRepository
    {
        public Polynomial ParsePolynomial(string raw)
        {
            string clean = StripCommandPrefix(raw.Trim());
            // Validar que AngouriMath puede parsearla
            Entity _ = clean;
            return new Polynomial { RawExpr = clean };
        }

        public ParsedDerivation BuildParsed(DerivationInput input, DerivationType type)
        {
            var parsed = new ParsedDerivation
            {
                Type       = type,
                Polynomial = ParsePolynomial(input.Raw),
                A          = input.A,
                B          = input.B,
                NthOrder   = input.NthOrder
            };

            if (!string.IsNullOrWhiteSpace(input.RawSecond))
                parsed.Second = ParsePolynomial(input.RawSecond!.TrimStart('/'));

            return parsed;
        }

        private static string StripCommandPrefix(string raw)
        {
            foreach (var prefix in new[] { "I", "C", "K", "R" })
                if (raw.StartsWith(prefix) && raw.Length > 1 && !char.IsLetter(raw[1]))
                    return raw[1..].Trim();
            return raw;
        }
    }
}
