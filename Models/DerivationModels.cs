namespace Models
{
    public enum DerivationType
    {
        Unknown,
        PowerRule,
        NthDerivative,
        ProductRule,
        QuotientRule,
        Integral,
        DefiniteIntegral,
        CriticalPoints,
        ConcavityAnalysis,
        RolleTheorem,
        EvaluatePoint
    }

    public class DerivationInput
    {
        public string  Raw       { get; set; } = "";
        public string? RawSecond { get; set; }
        public double? A         { get; set; }
        public double? B         { get; set; }
        public int     NthOrder  { get; set; } = 1;
    }

    public class ParsedDerivation
    {
        public DerivationType Type       { get; set; }
        public Polynomial     Polynomial { get; set; } = new();
        public Polynomial?    Second     { get; set; }
        public double?        A          { get; set; }
        public double?        B          { get; set; }
        public int            NthOrder   { get; set; } = 1;
    }

    public class DerivationOutput
    {
        public bool         Success     { get; set; }
        public string       Description { get; set; } = "";
        public string       Result      { get; set; } = "";
        public List<string> Steps       { get; set; } = new();
    }
}
