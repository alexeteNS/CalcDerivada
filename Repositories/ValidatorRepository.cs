using AngouriMath;
using Interfaces;

namespace Repositories
{
    public class ValidatorRepository : IValidatorRepository
    {
        public bool IsValidFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            try
            {
                string clean = StripCommandPrefix(input.Trim());
                Entity expr = clean;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsDerivable(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            try
            {
                string clean = StripCommandPrefix(input.Trim());
                Entity expr = clean;
                return expr.Vars.Contains(MathS.Var("x")) || expr.EvaluableNumerical;
            }
            catch
            {
                return false;
            }
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
