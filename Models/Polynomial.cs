namespace Models
{
    // Polynomial ahora es un contenedor ligero.
    // RawExpr almacena la expresión en string para que
    // AngouriMath la procese simbólicamente en DerivationRepository.
    // Terms y Evaluate se conservan por compatibilidad con interfaces existentes
    // pero ya no son el mecanismo principal de cálculo.
    public class Polynomial
    {
        public string RawExpr { get; set; } = "";

        // Compatibilidad legacy — ya no se usan para calcular
        public List<Term> Terms { get; set; } = new();

        public double Evaluate(double x)
        {
            // Delegado a AngouriMath vía DerivationRepository cuando se necesite
            throw new NotSupportedException("Usa DerivationRepository.EvaluatePoint en su lugar.");
        }

        public override string ToString() => RawExpr;
    }
}
