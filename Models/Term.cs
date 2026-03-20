namespace Models
{
    public class Term
    {
        public double Coefficient { get; set; }
        public double Exponent    { get; set; }

        public Term(double coefficient, double exponent)
        {
            Coefficient = coefficient;
            Exponent    = exponent;
        }

        public override string ToString()
        {
            if (Exponent == 0) return $"{Coefficient:G}";
            if (Exponent == 1) return $"{Coefficient:G}x";
            return $"{Coefficient:G}x^{Exponent:G}";
        }
    }
}
