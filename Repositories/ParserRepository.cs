using Interfaces;
using Models;

namespace Repositories
{
    public class ParserRepository : IParserRepository
    {
        public Polynomial ParsePolynomial(string raw)
        {
            string clean = raw.Replace(" ", "")
                              .TrimStart('I').TrimStart('C').TrimStart('K').TrimStart('R');

            clean = clean.Replace("-", "+-");
            if (clean.StartsWith("+")) clean = clean[1..];

            var poly = new Polynomial();
            foreach (var part in clean.Split('+'))
            {
                if (!string.IsNullOrEmpty(part))
                    poly.Terms.Add(ParseTerm(part));
            }
            return poly;
        }

        private Term ParseTerm(string s)
        {
            s = s.Trim();
            if (!s.Contains('x'))
                return new Term(double.Parse(s), 0);

            int    xIdx  = s.IndexOf('x');
            string left  = s[..xIdx];
            double coeff = left switch { "" or "+" => 1, "-" => -1, _ => double.Parse(left) };

            string right = s[(xIdx + 1)..];
            if (right.StartsWith("^"))
                return new Term(coeff, double.Parse(right[1..]));

            return new Term(coeff, 1);
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
    }
}
