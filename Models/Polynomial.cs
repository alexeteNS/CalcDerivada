namespace Models
{
    public class Polynomial
    {
        public List<Term> Terms { get; set; } = new();

        public double Evaluate(double x)
        {
            double result = 0;
            foreach (var t in Terms)
                result += t.Coefficient * Math.Pow(x, t.Exponent);
            return result;
        }

        public override string ToString()
        {
            var nonZero = Terms.FindAll(t => t.Coefficient != 0);
            if (nonZero.Count == 0) return "0";
            string result = nonZero[0].ToString();
            for (int i = 1; i < nonZero.Count; i++)
            {
                string part = nonZero[i].ToString();
                result += part.StartsWith("-") ? $" - {part.TrimStart('-')}" : $" + {part}";
            }
            return result;
        }
    }
}
