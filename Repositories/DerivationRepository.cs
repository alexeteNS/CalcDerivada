using Interfaces;
using MathNet.Symbolics;
using Models;
using Expr = MathNet.Symbolics.SymbolicExpression;
using Poly = Models.Polynomial;

namespace Repositories
{
    public class DerivationRepository : IDerivationRepository
    {
        // Variable simbólica x
        private static readonly Expr X = Expr.Variable("x");

        // ── helpers ──────────────────────────────────────────────────────────

        private static Expr Parse(string raw) => Expr.Parse(ValidatorRepository.Normalize(raw));

        private static Expr Derive(Expr expr, int times = 1)
        {
            var d = expr;
            for (int i = 0; i < times; i++)
                d = d.Differentiate(X);
            return d;
        }

        // Integración numérica por Simpson (MathNet.Symbolics no tiene integral simbólica)
        private static double NumericalIntegral(Expr expr, double a, double b, int n = 10000)
        {
            if (n % 2 != 0) n++;
            double h = (b - a) / n;
            double sum = EvalAt(expr, a) + EvalAt(expr, b);
            for (int i = 1; i < n; i++)
                sum += (i % 2 == 0 ? 2 : 4) * EvalAt(expr, a + i * h);
            return sum * h / 3;
        }

        // Antiderivada simbólica por regla de potencia (suficiente para polinomios)
        private static string AntiDerivativeStr(Expr expr)
        {
            // Derivar y formatear la antiderivada término a término vía diferenciación inversa
            // Usamos la representación infix para construir la antiderivada de un polinomio
            // expandiendo: ax^n -> ax^(n+1)/(n+1)
            return $"[antiderivada de {Fmt(expr)}]";
        }

        private static double EvalAt(Expr expr, double xVal)
        {
            var compiled = expr.Compile("x");
            return compiled(xVal);
        }

        private static List<double> FindRoots(Expr expr)
        {
            var roots = new List<double>();
            double step = 0.01;
            double prev = EvalAt(expr, -100);
            for (double v = -100 + step; v <= 100; v += step)
            {
                double curr = EvalAt(expr, v);
                if (!double.IsNaN(prev) && !double.IsNaN(curr) && prev * curr < 0)
                {
                    double root = Bisect(expr, v - step, v);
                    if (!roots.Exists(r => Math.Abs(r - root) < 0.05))
                        roots.Add(Math.Round(root, 4));
                }
                else if (!double.IsNaN(curr) && Math.Abs(curr) < 1e-6
                         && !roots.Exists(r => Math.Abs(r - v) < 0.05))
                    roots.Add(Math.Round(v, 4));
                prev = curr;
            }
            return roots;
        }

        private static double Bisect(Expr expr, double a, double b)
        {
            for (int i = 0; i < 60; i++)
            {
                double mid  = (a + b) / 2;
                double fMid = EvalAt(expr, mid);
                if (Math.Abs(fMid) < 1e-10) return mid;
                if (EvalAt(expr, a) * fMid < 0) b = mid; else a = mid;
            }
            return (a + b) / 2;
        }

        // Limpia el formato de salida: "3*x^2" -> "3x^2", espacía + y -
        private static string Fmt(Expr expr)
        {
            string s = Infix.Format(expr.Expression);
            // "3 * x" → "3x"
            s = System.Text.RegularExpressions.Regex.Replace(s, @"(\d)\s*\*\s*([a-zA-Z])", "$1$2");
            // "x ^ 2" → "x^2"
            s = System.Text.RegularExpressions.Regex.Replace(s, @"([a-zA-Z0-9])\s*\^\s*([\-a-zA-Z0-9]+)", "$1^$2");
            // espaciar operadores
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\s*\+\s*", " + ");
            s = System.Text.RegularExpressions.Regex.Replace(s, @"(?<![eE\d])\s*-\s*", " - ");
            return s.Trim();
        }

        // ── operaciones ──────────────────────────────────────────────────────

        public DerivationOutput PowerRule(Poly poly)
        {
            var f  = Parse(poly.RawExpr);
            var fp = Derive(f);
            return new DerivationOutput
            {
                Success     = true,
                Description = "Regla de la potencia",
                Result      = Fmt(fp),
                Steps       = new() { $"f(x)  = {Fmt(f)}", $"f'(x) = {Fmt(fp)}" }
            };
        }

        public DerivationOutput NthDerivative(Poly poly, int n)
        {
            var f     = Parse(poly.RawExpr);
            var steps = new List<string> { $"f(x) = {Fmt(f)}" };
            var cur   = f;
            for (int i = 1; i <= n; i++)
            {
                cur = Derive(cur);
                steps.Add($"f^({i})(x) = {Fmt(cur)}");
            }
            return new DerivationOutput
            {
                Success     = true,
                Description = $"Derivada de orden {n}",
                Result      = Fmt(cur),
                Steps       = steps
            };
        }

        public DerivationOutput ProductRule(Poly fPoly, Poly gPoly)
        {
            var f      = Parse(fPoly.RawExpr);
            var g      = Parse(gPoly.RawExpr);
            var df     = Derive(f);
            var dg     = Derive(g);
            var result = (df * g + f * dg);
            return new DerivationOutput
            {
                Success     = true,
                Description = "Regla del producto: (f·g)' = f'g + fg'",
                Result      = Fmt(result),
                Steps       = new()
                {
                    $"f(x)      = {Fmt(f)}",
                    $"g(x)      = {Fmt(g)}",
                    $"f'(x)     = {Fmt(df)}",
                    $"g'(x)     = {Fmt(dg)}",
                    $"f'·g      = {Fmt(df * g)}",
                    $"f·g'      = {Fmt(f * dg)}",
                    $"(f·g)'(x) = {Fmt(result)}"
                }
            };
        }

        public DerivationOutput QuotientRule(Poly fPoly, Poly gPoly)
        {
            var f   = Parse(fPoly.RawExpr);
            var g   = Parse(gPoly.RawExpr);
            var df  = Derive(f);
            var dg  = Derive(g);
            var num = df * g - f * dg;
            var den = g * g;
            return new DerivationOutput
            {
                Success     = true,
                Description = "Regla del cociente: (f/g)' = (f'g - fg') / g²",
                Result      = $"({Fmt(num)}) / ({Fmt(den)})",
                Steps       = new()
                {
                    $"f(x)        = {Fmt(f)}",
                    $"g(x)        = {Fmt(g)}",
                    $"f'(x)       = {Fmt(df)}",
                    $"g'(x)       = {Fmt(dg)}",
                    $"Numerador   = {Fmt(num)}",
                    $"Denominador = {Fmt(den)}"
                }
            };
        }

        public DerivationOutput Integral(Poly poly)
        {
            // MathNet.Symbolics diferencia pero no integra simbólicamente.
            // Calculamos la antiderivada derivando hacia atrás: ax^n -> ax^(n+1)/(n+1)
            // usando la representación de términos del polinomio.
            var f        = Parse(poly.RawExpr);
            string anti  = BuildAntiderivative(poly.RawExpr);
            return new DerivationOutput
            {
                Success     = true,
                Description = "Integral indefinida: ∫f(x)dx",
                Result      = $"{anti} + C",
                Steps       = new() { $"f(x)    = {Fmt(f)}", $"∫f(x)dx = {anti} + C" }
            };
        }

        public DerivationOutput DefiniteIntegral(Poly poly, double a, double b)
        {
            var f   = Parse(poly.RawExpr);
            double val = NumericalIntegral(f, a, b);
            return new DerivationOutput
            {
                Success     = true,
                Description = $"Integral definida [{a}, {b}]",
                Result      = $"{val:G8}",
                Steps       = new()
                {
                    $"f(x)                = {Fmt(f)}",
                    $"Método              : Simpson 1/3 (numérico)",
                    $"∫f(x)dx de {a} a {b} = {val:G8}"
                }
            };
        }

        public DerivationOutput CriticalPoints(Poly poly)
        {
            var f    = Parse(poly.RawExpr);
            var fp   = Derive(f);
            var fpp  = Derive(fp);
            var roots = FindRoots(fp);

            var steps = new List<string>
            {
                $"f(x)   = {Fmt(f)}",
                $"f'(x)  = {Fmt(fp)}",
                $"f''(x) = {Fmt(fpp)}",
                ""
            };

            if (roots.Count == 0)
                steps.Add("No se encontraron puntos críticos en [-100, 100]");
            else
                foreach (var x in roots)
                {
                    double y     = EvalAt(f, x);
                    double fppx  = EvalAt(fpp, x);
                    string tipo  = fppx < 0 ? "Máximo local"
                                 : fppx > 0 ? "Mínimo local"
                                 : "Silla / inflexión";
                    steps.Add($"x = {x:G4}  |  f(x) = {y:G4}  |  f''(x) = {fppx:G4}  →  {tipo}");
                }

            return new DerivationOutput
            {
                Success     = true,
                Description = "Puntos críticos: f'(x) = 0",
                Result      = roots.Count == 0 ? "Ninguno" : string.Join(", ", roots.Select(r => $"x={r:G4}")),
                Steps       = steps
            };
        }

        public DerivationOutput ConcavityAnalysis(Poly poly)
        {
            var f           = Parse(poly.RawExpr);
            var fp          = Derive(f);
            var fpp         = Derive(fp);
            var inflections = FindRoots(fpp);

            var steps = new List<string>
            {
                $"f(x)   = {Fmt(f)}",
                $"f'(x)  = {Fmt(fp)}",
                $"f''(x) = {Fmt(fpp)}",
                ""
            };

            if (inflections.Count > 0)
            {
                steps.Add("Puntos de inflexión (f''(x) = 0):");
                foreach (var x in inflections)
                    steps.Add($"  x = {x:G4}  |  f(x) = {EvalAt(f, x):G4}");
                steps.Add("");
            }

            steps.Add("Concavidad en puntos de muestra:");
            foreach (double sx in new double[] { -10, -5, -1, 0, 1, 5, 10 })
            {
                if (inflections.Exists(ix => Math.Abs(ix - sx) < 1)) continue;
                double v     = EvalAt(fpp, sx);
                string label = v < 0 ? "Cóncavo ↓ (hacia abajo)"
                             : v > 0 ? "Convexo ↑ (hacia arriba)"
                             : "Inflexión";
                steps.Add($"  x = {sx}  |  f''(x) = {v:G4}  →  {label}");
            }

            return new DerivationOutput
            {
                Success     = true,
                Description = "Análisis de concavidad y convexidad",
                Result      = $"f''(x) = {Fmt(fpp)}",
                Steps       = steps
            };
        }

        public DerivationOutput RolleTheorem(Poly poly, double a, double b)
        {
            var f  = Parse(poly.RawExpr);
            var fp = Derive(f);
            double fa = EvalAt(f, a);
            double fb = EvalAt(f, b);

            var steps = new List<string>
            {
                $"f(x)      = {Fmt(f)}",
                $"f'(x)     = {Fmt(fp)}",
                $"Intervalo : [{a}, {b}]",
                $"f({a})    = {fa:G6}",
                $"f({b})    = {fb:G6}",
                ""
            };

            if (Math.Abs(fa - fb) > 1e-6)
            {
                steps.Add($"✘ f({a}) ≠ f({b}) — El Teorema de Rolle NO aplica.");
                return new DerivationOutput
                {
                    Success     = false,
                    Description = "Teorema de Rolle",
                    Result      = "No aplica",
                    Steps       = steps
                };
            }

            var roots = FindRoots(fp).FindAll(c => c > a && c < b);
            steps.Add($"✔ f({a}) = f({b}) = {fa:G6}");
            steps.Add("");

            if (roots.Count == 0)
                steps.Add($"No se halló c ∈ ({a},{b}) donde f'(c) = 0");
            else
            {
                steps.Add($"Existe(n) c ∈ ({a},{b}) donde f'(c) = 0:");
                foreach (var c in roots)
                    steps.Add($"  c = {c:G4}  |  f(c) = {EvalAt(f, c):G4}");
            }

            return new DerivationOutput
            {
                Success     = true,
                Description = "Teorema de Rolle",
                Result      = roots.Count > 0
                    ? string.Join(", ", roots.Select(c => $"c={c:G4}"))
                    : "No encontrado",
                Steps       = steps
            };
        }

        public DerivationOutput EvaluatePoint(Poly poly, double x)
        {
            var f   = Parse(poly.RawExpr);
            var fp  = Derive(f);
            var fpp = Derive(fp);
            return new DerivationOutput
            {
                Success     = true,
                Description = $"Evaluación en x = {x}",
                Result      = $"f({x}) = {EvalAt(f, x):G6}",
                Steps       = new()
                {
                    $"f(x)     = {Fmt(f)}",
                    $"f'(x)    = {Fmt(fp)}",
                    $"f''(x)   = {Fmt(fpp)}",
                    "",
                    $"f({x})   = {EvalAt(f,  x):G6}",
                    $"f'({x})  = {EvalAt(fp, x):G6}",
                    $"f''({x}) = {EvalAt(fpp,x):G6}"
                }
            };
        }

        // Antiderivada de un polinomio en x: ax^n → ax^(n+1)/(n+1)
        // Funciona para polinomios estándar ingresados por el usuario.
        private static string BuildAntiderivative(string rawExpr)
        {
            // Normalizamos: agregamos * explícito entre coeficiente y x, luego derivamos la inversa
            // Estrategia: f'(g) = expr => g = antiderivada
            // Aproximación simbólica: hacemos x → x con grado+1 via términos
            try
            {
                // Usamos la derivada simbólica en reverse: probamos g tal que g' = expr
                // Para polinomios esto es directo: integrar = sumar grado y dividir coeff
                var terms = ParseToTerms(rawExpr);
                var parts = new List<string>();
                foreach (var (coeff, exp) in terms)
                {
                    double newExp   = exp + 1;
                    double newCoeff = coeff / newExp;
                    if (newExp == 0) continue;
                    string c = newCoeff == 1 ? "" : newCoeff == -1 ? "-" : $"{newCoeff:G}";
                    string t = newExp == 1 ? $"{c}x"
                             : newExp == 0 ? $"{newCoeff:G}"
                             : $"{c}x^{newExp:G}";
                    parts.Add(t);
                }
                if (parts.Count == 0) return "0";
                string result = parts[0];
                for (int i = 1; i < parts.Count; i++)
                    result += parts[i].StartsWith("-") ? $" - {parts[i].TrimStart('-')}" : $" + {parts[i]}";
                return result;
            }
            catch
            {
                return $"∫({rawExpr})dx";
            }
        }

        // Parsea un polinomio simple en lista de (coeficiente, exponente)
        private static List<(double coeff, double exp)> ParseToTerms(string raw)
        {
            var result = new List<(double, double)>();
            // Normalizar: reemplazar - por +- para split fácil
            string s = raw.Replace(" ", "").Replace("-", "+-");
            if (s.StartsWith("+")) s = s[1..];
            foreach (var part in s.Split('+'))
            {
                if (string.IsNullOrEmpty(part)) continue;
                if (!part.Contains('x'))
                {
                    result.Add((double.Parse(part), 0));
                    continue;
                }
                int xi    = part.IndexOf('x');
                string lf = part[..xi];
                double c  = lf switch { "" or "+" => 1, "-" => -1, _ => double.Parse(lf) };
                string rf = part[(xi + 1)..];
                double e  = rf.StartsWith("^") ? double.Parse(rf[1..]) : 1;
                result.Add((c, e));
            }
            return result;
        }
    }
}
