using Interfaces;
using Models;
using Repositories;
using Services;

IValidatorService  validatorService  = new ValidatorService(new ValidatorRepository());
IDetectorService   detectorService   = new DetectorService(new DetectorRepository());
IParserService     parserService     = new ParserService(new ParserRepository());
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

    // PASO 1 - validatorService recibe el input y le pregunta al ValidatorRepository si el formato es valido y si es derivable
    bool isValid = validatorService.Validate(input);
    Console.WriteLine("Paso 1 - Validacion: " + (isValid ? "valido" : "invalido - " + validatorService.ValidationError));
    if (!isValid) { Console.WriteLine(); continue; }

    // PASO 2 - detectorService recibe el input y le pregunta al DetectorRepository que tipo de derivacion es
    DerivationType type = detectorService.Detect(input);
    Console.WriteLine("Paso 2 - Tipo detectado: " + type);

    // PASO 3 - parserService recibe el input y el tipo, le pide al ParserRepository que convierta el string a Polynomial (vía AngouriMath)
    ParsedDerivation parsed = parserService.Parse(input, type);
    Console.WriteLine("Paso 3 - Parseado: " + parsed.Polynomial);

    // PASO 4 - derivationService recibe el parsed, llama al DerivationRepository que resuelve la operacion con AngouriMath y regresa el resultado
    DerivationOutput output = derivationService.Solve(parsed);
    Console.WriteLine("Paso 4 - Resultado: " + output.Result);

    Console.WriteLine();
    foreach (var step in output.Steps)
        Console.WriteLine(step);

    Console.WriteLine();
}
