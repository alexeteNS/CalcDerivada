using Interfaces;
using Models;

namespace Repositories
{
    public class DerivationRepository : IDerivationRepository
    {
        public DerivationOutput PowerRule(Polynomial poly)
        {
            var d = Derive(poly);
            return new DerivationOutput
            {
                Success     = true,
                Description = "Regla de la potencia",
                Result      = d.ToString(),
                Steps       = new() { $"f(x)  = {poly}", $"f'(x) = {d}" }
            };
        }

        public DerivationOutput NthDerivative(Polynomial poly, int n)
        {
            var steps   = new List<string> { $"f(x) = {poly}" };
            var current = poly;
            for (int i = 1; i <= n; i++)
            {
                current = Derive(current);
                steps.Add($"f^({i})(x) = {current}");
            }
            return new DerivationOutput
            {
                Success     = true,
                Description = $"Derivada de orden {n}",
                Result      = current.ToString(),
                Steps       = steps
            };
        }

        public DerivationOutput ProductRule(Polynomial f, Polynomial g)
        {
            var df     = Derive(f);
            var dg     = Derive(g);
            var result = Add(Multiply(df, g), Multiply(f, dg));
            return new DerivationOutput
            {
                Success     = true,
                Description = "Regla del producto: (f·g)' = f'g + fg'",
                Result      = result.ToString(),
                Steps       = new()
                {
                    $"f(x)      = {f}",
                    $"g(x)      = {g}",
                    $"f'(x)     = {df}",
                    $"g'(x)     = {dg}",
                    $"f'·g      = {Multiply(df, g)}",
                    $"f·g'      = {Multiply(f, dg)}",
                    $"(f·g)'(x) = {result}"
                }
            };
        }

        public DerivationOutput QuotientRule(Polynomial f, Polynomial g)
        {
            var df  = Derive(f);
            var dg  = Derive(g);
            var num = Subtract(Multiply(df, g), Multiply(f, dg));
            var den = Multiply(g, g);
            return new DerivationOutput
            {
                Success     = true,
                Description = "Regla del cociente: (f/g)' = (f'g - fg') / g²",
                Result      = $"({num}) / ({den})",
                Steps       = new()
                {
                    $"f(x)       = {f}",
                    $"g(x)       = {g}",
                    $"f'(x)      = {df}",
                    $"g'(x)      = {dg}",
                    $"Numerador  = {num}",
                    $"Denominador= {den}"
                }
            };
        }

        public DerivationOutput Integral(Polynomial poly)
        {
            var result = Integrate(poly);
            return new DerivationOutput
            {
                Success     = true,
                Description = "Integral indefinida: ∫ax^n dx = ax^(n+1)/(n+1) + C",
                Result      = $"{result} + C",
                Steps       = new() { $"f(x)    = {poly}", $"∫f(x)dx = {result} + C" }
            };
        }

        public DerivationOutput DefiniteIntegral(Polynomial poly, double a, double b)
        {
            var anti = Integrate(poly);
            double val = anti.Evaluate(b) - anti.Evaluate(a);
            return new DerivationOutput
            {
                Success     = true,
                Description = $"Integral definida [{a}, {b}]",
                Result      = $"{val:G8}",
                Steps       = new()
                {
                    $"f(x)          = {poly}",
                    $"F(x)          = {anti} + C",
                    $"F({b})        = {anti.Evaluate(b):G8}",
                    $"F({a})        = {anti.Evaluate(a):G8}",
                    $"F({b}) - F({a}) = {val:G8}"
                }
            };
        }

        public DerivationOutput CriticalPoints(Polynomial poly)
        {
            var fp  = Derive(poly);
            var fpp = Derive(fp);
            var roots = FindRoots(fp);

            var steps = new List<string> { $"f(x)   = {poly}", $"f'(x)  = {fp}", $"f''(x) = {fpp}", "" };

            if (roots.Count == 0)
            {
                steps.Add("No se encontraron puntos críticos en [-100, 100]");
            }
            else
            {
                foreach (var x in roots)
                {
                    double y   = poly.Evaluate(x);
                    double fpp_x = fpp.Evaluate(x);
                    string tipo = fpp_x < 0 ? "Máximo local"
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
            var fp  = Derive(poly);
            var fpp = Derive(fp);
            var inflections = FindRoots(fpp);

            var steps = new List<string> { $"f(x)   = {poly}", $"f'(x)  = {fp}", $"f''(x) = {fpp}", "" };

            if (inflections.Count > 0)
            {
                steps.Add("Puntos de inflexión (f''(x) = 0):");
                foreach (var x in inflections)
                    steps.Add($"  x = {x:G4}  |  f(x) = {poly.Evaluate(x):G4}");
                steps.Add("");
            }

            steps.Add("Concavidad en puntos de muestra:");
            foreach (double sx in new double[] { -10, -5, -1, 0, 1, 5, 10 })
            {
                if (inflections.Exists(ix => Math.Abs(ix - sx) < 1)) continue;
                double v     = fpp.Evaluate(sx);
                string label = v < 0 ? "Cóncavo ↓ (hacia abajo)" : v > 0 ? "Convexo ↑ (hacia arriba)" : "Inflexión";
                steps.Add($"  x = {sx}  |  f''(x) = {v:G4}  →  {label}");
            }

            return new DerivationOutput
            {
                Success     = true,
                Description = "Análisis de concavidad y convexidad",
                Result      = $"f''(x) = {fpp}",
                Steps       = steps
            };
        }

        public DerivationOutput RolleTheorem(Polynomial poly, double a, double b)
        {
            var fp = Derive(poly);
            double fa = poly.Evaluate(a);
            double fb = poly.Evaluate(b);

            var steps = new List<string>
            {
                $"f(x)      = {poly}",
                $"f'(x)     = {fp}",
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
            {
                steps.Add($"No se halló c ∈ ({a},{b}) donde f'(c) = 0");
            }
            else
            {
                steps.Add($"Existe(n) c ∈ ({a},{b}) donde f'(c) = 0:");
                foreach (var c in roots)
                    steps.Add($"  c = {c:G4}  |  f(c) = {poly.Evaluate(c):G4}");
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
            var fp  = Derive(poly);
            var fpp = Derive(fp);
            return new DerivationOutput
            {
                Success     = true,
                Description = $"Evaluación en x = {x}",
                Result      = $"f({x}) = {poly.Evaluate(x):G6}",
                Steps       = new()
                {
                    $"f(x)     = {poly}",
                    $"f'(x)    = {fp}",
                    $"f''(x)   = {fpp}",
                    "",
                    $"f({x})   = {poly.Evaluate(x):G6}",
                    $"f'({x})  = {fp.Evaluate(x):G6}",
                    $"f''({x}) = {fpp.Evaluate(x):G6}"
                }
            };
        }

        private Polynomial Derive(Polynomial poly)
        {
            var r = new Polynomial();
            foreach (var t in poly.Terms)
                if (t.Exponent != 0)
                    r.Terms.Add(new Term(t.Coefficient * t.Exponent, t.Exponent - 1));
            return r;
        }

        private Polynomial Integrate(Polynomial poly)
        {
            var r = new Polynomial();
            foreach (var t in poly.Terms)
                r.Terms.Add(new Term(t.Coefficient / (t.Exponent + 1), t.Exponent + 1));
            return r;
        }

        private Polynomial Add(Polynomial a, Polynomial b)
        {
            var r = new Polynomial();
            r.Terms.AddRange(a.Terms.Select(t => new Term(t.Coefficient, t.Exponent)));
            r.Terms.AddRange(b.Terms.Select(t => new Term(t.Coefficient, t.Exponent)));
            return Simplify(r);
        }

        private Polynomial Subtract(Polynomial a, Polynomial b)
        {
            var neg = new Polynomial();
            neg.Terms.AddRange(b.Terms.Select(t => new Term(-t.Coefficient, t.Exponent)));
            return Add(a, neg);
        }

        private Polynomial Multiply(Polynomial a, Polynomial b)
        {
            var r = new Polynomial();
            foreach (var ta in a.Terms)
                foreach (var tb in b.Terms)
                    r.Terms.Add(new Term(ta.Coefficient * tb.Coefficient, ta.Exponent + tb.Exponent));
            return Simplify(r);
        }

        private Polynomial Simplify(Polynomial poly)
        {
            var groups = new Dictionary<double, double>();
            foreach (var t in poly.Terms)
            {
                if (groups.ContainsKey(t.Exponent)) groups[t.Exponent] += t.Coefficient;
                else groups[t.Exponent] = t.Coefficient;
            }
            var r = new Polynomial();
            foreach (var kv in groups.OrderByDescending(k => k.Key))
                if (Math.Abs(kv.Value) > 1e-12)
                    r.Terms.Add(new Term(kv.Value, kv.Key));
            return r;
        }

        private List<double> FindRoots(Polynomial poly)
        {
            var roots = new List<double>();
            double step = 0.001;
            double prev = poly.Evaluate(-100);
            for (double x = -100 + step; x <= 100; x += step)
            {
                double curr = poly.Evaluate(x);
                if (prev * curr < 0)
                {
                    double root = Bisect(poly, x - step, x);
                    if (!roots.Exists(r => Math.Abs(r - root) < 0.01))
                        roots.Add(Math.Round(root, 4));
                }
                else if (Math.Abs(curr) < 1e-7 && !roots.Exists(r => Math.Abs(r - x) < 0.01))
                    roots.Add(Math.Round(x, 4));
                prev = curr;
            }
            return roots;
        }

        private double Bisect(Polynomial poly, double a, double b)
        {
            for (int i = 0; i < 60; i++)
            {
                double mid  = (a + b) / 2;
                double fMid = poly.Evaluate(mid);
                if (Math.Abs(fMid) < 1e-10) return mid;
                if (poly.Evaluate(a) * fMid < 0) b = mid; else a = mid;
            }
            return (a + b) / 2;
        }
    }
}
