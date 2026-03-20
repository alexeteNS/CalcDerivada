namespace Models
{
    // Contenedor simple: guarda la expresión como string.
    // MathNet.Symbolics la procesa en DerivationRepository.
    public class Polynomial
    {
        public string RawExpr { get; set; } = "";
        public override string ToString() => RawExpr;
    }
}
