using Interfaces;

namespace Repositories
{
    public class ValidatorRepository : IValidatorRepository
    {
        public bool IsValidFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            foreach (char c in input.Replace(" ", ""))
                if (!char.IsDigit(c) && c != 'x' && c != '^' && c != '+'
                    && c != '-' && c != '.' && c != '/' && c != '*')
                    return false;
            return true;
        }

        public bool IsDerivable(string input)
        {
            string clean = input.Replace(" ", "");
            return clean.Contains('x') || double.TryParse(clean, out _);
        }
    }
}
