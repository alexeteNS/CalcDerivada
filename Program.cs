using Interfaces;
using Models;
using Repositories;
using Services;

IValidatorService  validatorService  = new ValidatorService(new ValidatorRepository());
IDerivationService derivationService = new DerivationService(new DerivationRepository());

while (true)
{
    Console.Write("Expresion: ");
    string raw = Console.ReadLine() ?? "";
    if (raw.Trim().ToLower() == "salir") break;

    var input = new DerivationInput { Raw = raw.Trim() };

    Console.Write("Segundo polinomio (Enter omite): ");
    string second = Console.ReadLine() ?? "";
    if (!string.IsNullOrWhiteSpace(second)) input.RawSecond = second.Trim();

    Console.Write("Orden de derivada (Enter = 1): ");
    if (int.TryParse(Console.ReadLine(), out int nth) && nth > 1) input.NthOrder = nth;

    Console.Write("Valor de a (Enter omite): ");
    string aStr = Console.ReadLine() ?? "";
    if (double.TryParse(aStr, out double a))
    {
        input.A = a;
        Console.Write("Valor de b (Enter omite): ");
        if (double.TryParse(Console.ReadLine(), out double b)) input.B = b;
    }

    Console.WriteLine();

    // PASO 1 — ValidatorService valida formato y derivabilidad
    bool isValid = validatorService.Validate(input);
    Console.WriteLine("Paso 1 - Validacion: " + (isValid ? "valido" : "invalido - " + validatorService.ValidationError));
    if (!isValid) { Console.WriteLine(); continue; }

    // PASO 2 — Detectar tipo y construir ParsedDerivation directamente
    DerivationType type = DetectType(input);
    Console.WriteLine("Paso 2 - Tipo detectado: " + type);

    var parsed = new ParsedDerivation
    {
        Type       = type,
        Polynomial = new Polynomial { RawExpr = StripPrefix(input.Raw) },
        A          = input.A,
        B          = input.B,
        NthOrder   = input.NthOrder
    };
    if (!string.IsNullOrWhiteSpace(input.RawSecond))
        parsed.Second = new Polynomial { RawExpr = input.RawSecond!.TrimStart('/').Trim() };

    Console.WriteLine("Paso 3 - Parseado: " + parsed.Polynomial);

    // PASO 3 — DerivationService resuelve con MathNet.Symbolics
    DerivationOutput output = derivationService.Solve(parsed);
    Console.WriteLine("Paso 4 - Resultado: " + output.Result);

    Console.WriteLine();
    foreach (var step in output.Steps)
        Console.WriteLine(step);

    Console.WriteLine();
}

// ── helpers locales ───────────────────────────────────────────────────────────

static DerivationType DetectType(DerivationInput input)
{
    string raw = input.Raw.Trim();

    if (input.NthOrder > 1)                                               return DerivationType.NthDerivative;
    if (!string.IsNullOrWhiteSpace(input.RawSecond))
        return input.RawSecond!.TrimStart().StartsWith("/")
            ? DerivationType.QuotientRule : DerivationType.ProductRule;
    if (raw.StartsWith("R") && input.A.HasValue && input.B.HasValue)      return DerivationType.RolleTheorem;
    if (raw.StartsWith("I") && input.A.HasValue && input.B.HasValue)      return DerivationType.DefiniteIntegral;
    if (raw.StartsWith("I"))                                               return DerivationType.Integral;
    if (raw.StartsWith("C"))                                               return DerivationType.CriticalPoints;
    if (raw.StartsWith("K"))                                               return DerivationType.ConcavityAnalysis;
    if (input.A.HasValue && !input.B.HasValue)                             return DerivationType.EvaluatePoint;
    return DerivationType.PowerRule;
}

static string StripPrefix(string raw)
{
    foreach (var p in new[] { "I", "C", "K", "R" })
        if (raw.StartsWith(p) && raw.Length > 1 && !char.IsLetter(raw[1]))
            return raw[1..].Trim();
    return raw;
}
