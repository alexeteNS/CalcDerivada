using AngouriMath;
using AngouriMath.Extensions;
using Interfaces;
using Models;
using static AngouriMath.MathS;
using System.Text.RegularExpressions;

namespace Repositories
{
    public class DerivationRepository : IDerivationRepository
    {
        private static readonly Entity.Variable X = (Entity.Variable)"x";

        // ── helpers ──────────────────────────────────────────────────────────

        // Limpia la representación de AngouriMath:
        //   "3 * x ^ 2" → "3x^2",  "6 * x - 3" → "6x - 3"
        private static string Fmt(Entity expr)
        {
            string s = expr.ToString();
            // "3 * x" → "3x"
            s = Regex.Replace(s, @"(\d)\s*\*\s*([a-zA-Z])", "$1$2");
            // "x ^ 2" → "x^2"
            s = Regex.Replace(s, @"([a-zA-Z0-9])\s*\^\s*([\-a-zA-Z0-9]+)", "$1^$2");
            // espaciar operadores para legibilidad
            s = Regex.Replace(s, @"\s*\+\s*", " + ");
            s = Regex.Replace(s, @"\s*-\s*", " - ");
            // limpiar * restantes (producto de polinomios)
            s = Regex.Replace(s, @"\s*\*\s*", "*");
            return s.Trim();
        }

        private static Entity Parse(string raw) => (Entity)raw;

        private static Entity Derive(Entity expr, int times = 1)
        {
            Entity d = expr;
            for (int i = 0; i < times; i++)
                d = d.Differentiate(X).Expand().Simplify();
            return d;
        }

        private static Entity Integrate(Entity expr) =>
            expr.Integrate(X).Expand().Simplify();

        private static double EvalAt(Entity expr, double x)
        {
            var result = expr.Substitute(X, x).EvalNumerical();
            return (double)result.RealPart;
        }

        private static List<double> FindRoots(Entity expr)
        {
            var roots = new List<double>();
            double step = 0.01;
            double prev = EvalAt(expr, -100);
            for (double val = -100 + step; val <= 100; val += step)
            {
                double curr = EvalAt(expr, val);
                if (prev * curr < 0)
                {
                    double root = Bisect(expr, val - step, val);
                    if (!roots.Exists(r => Math.Abs(r - root) < 0.05))
                        roots.Add(Math.Round(root, 4));
                }
                else if (Math.Abs(curr) < 1e-6 && !roots.Exists(r => Math.Abs(r - val) < 0.05))
                    roots.Add(Math.Round(val, 4));
                prev = curr;
            }
            return roots;
        }

        private static double Bisect(Entity expr, double a, double b)
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

        // ── operaciones ──────────────────────────────────────────────────────

        public DerivationOutput PowerRule(Polynomial poly)
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

        public DerivationOutput NthDerivative(Polynomial poly, int n)
        {
            var f     = Parse(poly.RawExpr);
            var steps = new List<string> { $"f(x) = {Fmt(f)}" };
            Entity current = f;
            for (int i = 1; i <= n; i++)
            {
                current = Derive(current);
                steps.Add($"f^({i})(x) = {Fmt(current)}");
            }
            return new DerivationOutput
            {
                Success     = true,
                Description = $"Derivada de orden {n}",
                Result      = Fmt(current),
                Steps       = steps
            };
        }

        public DerivationOutput ProductRule(Polynomial fPoly, Polynomial gPoly)
        {
            var f      = Parse(fPoly.RawExpr);
            var g      = Parse(gPoly.RawExpr);
            var df     = Derive(f);
            var dg     = Derive(g);
            var result = (df * g + f * dg).Expand().Simplify();
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
                    $"f'·g      = {Fmt((df * g).Expand().Simplify())}",
                    $"f·g'      = {Fmt((f * dg).Expand().Simplify())}",
                    $"(f·g)'(x) = {Fmt(result)}"
                }
            };
        }

        public DerivationOutput QuotientRule(Polynomial fPoly, Polynomial gPoly)
        {
            var f   = Parse(fPoly.RawExpr);
            var g   = Parse(gPoly.RawExpr);
            var df  = Derive(f);
            var dg  = Derive(g);
            var num = (df * g - f * dg).Expand().Simplify();
            var den = (g * g).Expand().Simplify();
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

        public DerivationOutput Integral(Polynomial poly)
        {
            var f      = Parse(poly.RawExpr);
            var result = Integrate(f);
            return new DerivationOutput
            {
                Success     = true,
                Description = "Integral indefinida: ∫f(x)dx",
                Result      = $"{Fmt(result)} + C",
                Steps       = new() { $"f(x)    = {Fmt(f)}", $"∫f(x)dx = {Fmt(result)} + C" }
            };
        }

        public DerivationOutput DefiniteIntegral(Polynomial poly, double a, double b)
        {
            var f    = Parse(poly.RawExpr);
            var anti = Integrate(f);
            double fb = EvalAt(anti, b);
            double fa = EvalAt(anti, a);
            double val = fb - fa;
            return new DerivationOutput
            {
                Success     = true,
                Description = $"Integral definida [{a}, {b}]",
                Result      = $"{val:G8}",
                Steps       = new()
                {
                    $"f(x)            = {Fmt(f)}",
                    $"F(x)            = {Fmt(anti)} + C",
                    $"F({b})          = {fb:G8}",
                    $"F({a})          = {fa:G8}",
                    $"F({b}) - F({a}) = {val:G8}"
                }
            };
        }

        public DerivationOutput CriticalPoints(Polynomial poly)
        {
            var f    = Parse(poly.RawExpr);
            var fp   = Derive(f);
            var fpp  = Derive(fp);
            var roots = FindRoots(fp);

            var steps = new List<string> { $"f(x)   = {Fmt(f)}", $"f'(x)  = {Fmt(fp)}", $"f''(x) = {Fmt(fpp)}", "" };

            if (roots.Count == 0)
            {
                steps.Add("No se encontraron puntos críticos en [-100, 100]");
            }
            else
            {
                foreach (var x in roots)
                {
                    double y      = EvalAt(f, x);
                    double fpp_x  = EvalAt(fpp, x);
                    string tipo   = fpp_x < 0 ? "Máximo local"
                                  : fpp_x > 0 ? "Mínimo local"
                                  : "Silla / inflexión";
                    steps.Add($"x = {x:G4}  |  f(x) = {y:G4}  |  f''(x) = {fpp_x:G4}  →  {tipo}");
                }
            }

            return new DerivationOutput
            {
                Success     = true,
                Description = "Puntos críticos: f'(x) = 0",
                Result      = roots.Count == 0 ? "Ninguno" : string.Join(", ", roots.Select(r => $"x={r:G4}")),
                Steps       = steps
            };
        }

        public DerivationOutput ConcavityAnalysis(Polynomial poly)
        {
            var f           = Parse(poly.RawExpr);
            var fp          = Derive(f);
            var fpp         = Derive(fp);
            var inflections = FindRoots(fpp);

            var steps = new List<string> { $"f(x)   = {Fmt(f)}", $"f'(x)  = {Fmt(fp)}", $"f''(x) = {Fmt(fpp)}", "" };

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
                string label = v < 0 ? "Cóncavo ↓ (hacia abajo)" : v > 0 ? "Convexo ↑ (hacia arriba)" : "Inflexión";
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

        public DerivationOutput RolleTheorem(Polynomial poly, double a, double b)
        {
            var f  = Parse(poly.RawExpr);
            var fp = Derive(f);
            double fa = EvalAt(f, a);
            double fb = EvalAt(f, b);

            var steps = new List<string>
            {
                $"f(x)       = {Fmt(f)}",
                $"f'(x)      = {Fmt(fp)}",
                $"Intervalo  : [{a}, {b}]",
                $"f({a})     = {fa:G6}",
                $"f({b})     = {fb:G6}",
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
                Result      = roots.Count > 0 ? string.Join(", ", roots.Select(c => $"c={c:G4}")) : "No encontrado",
                Steps       = steps
            };
        }

        public DerivationOutput EvaluatePoint(Polynomial poly, double x)
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
                    $"f({x})   = {EvalAt(f, x):G6}",
                    $"f'({x})  = {EvalAt(fp, x):G6}",
                    $"f''({x}) = {EvalAt(fpp, x):G6}"
                }
            };
        }
    }
}
