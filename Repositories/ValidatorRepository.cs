using Interfaces;
using MathNet.Symbolics;
using System.Text.RegularExpressions;

namespace Repositories
{
    public class ValidatorRepository : IValidatorRepository
    {
        // Caracteres permitidos: dígitos, x, operadores, paréntesis, punto, espacios
        private static readonly Regex AllowedChars  = new(@"^[\d\sx\+\-\*\^\/\.\(\)]+$");
        // Patrón inválido: dígito pegado DESPUÉS de x  (x3, x12)
        private static readonly Regex InvalidXDigit = new(@"x\d");

        public bool IsValidFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string clean = StripCommandPrefix(input.Trim());

            if (!AllowedChars.IsMatch(clean))  return false;
            if (InvalidXDigit.IsMatch(clean))  return false;

            try
            {
                // Normalizar multiplicación implícita antes de pasar a MathNet
                Infix.ParseOrThrow(Normalize(clean));
                return true;
            }
            catch { return false; }
        }

        public bool IsDerivable(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            try
            {
                Infix.ParseOrThrow(Normalize(StripCommandPrefix(input.Trim())));
                return true;  // si parsea, es derivable
            }
            catch { return false; }
        }

        // Inserta * donde hay multiplicación implícita:
        //   "2x"   → "2*x"
        //   "2x^2" → "2*x^2"
        //   "3(x)" → "3*(x)"
        //   "(x+1)(x-1)" → "(x+1)*(x-1)"
        public static string Normalize(string expr)
        {
            string s = expr.Replace(" ", "");
            // número seguido de x:       2x → 2*x
            s = Regex.Replace(s, @"(\d)(x)", "$1*$2");
            // número seguido de (:       3( → 3*(
            s = Regex.Replace(s, @"(\d)(\()", "$1*$2");
            // ) seguido de x o (:        )(  → )*(   )(x → )*x
            s = Regex.Replace(s, @"(\))([x\(])", "$1*$2");
            return s;
        }

        private static string StripCommandPrefix(string raw)
        {
            foreach (var p in new[] { "I", "C", "K", "R" })
                if (raw.StartsWith(p) && raw.Length > 1 && !char.IsLetter(raw[1]))
                    return raw[1..].Trim();
            return raw;
        }
    }
}
