using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

public static class BuffFormula
{
    private static readonly string[] Operators = ["+=", "-=", "*=", "/="];

    /// Parse "str+=1" or "health*=dex/2" into (target, op, expr).
    public static (string target, string op, string expr)? Parse(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula)) return null;
        foreach (var op in Operators)
        {
            var idx = formula.IndexOf(op);
            if (idx > 0)
            {
                var target = formula[..idx].Trim().ToLower();
                var expr = formula[(idx + op.Length)..].Trim();
                if (target.Length > 0 && expr.Length > 0)
                    return (target, op, expr);
            }
        }
        return null;
    }

    /// Build a variable context from a character's stats and gauges.
    public static Dictionary<string, double> BuildContext(StatBlock? sb, IEnumerable<Gauge>? gauges)
    {
        var ctx = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        if (sb?.Stats != null)
            foreach (var s in sb.Stats)
                if (!string.IsNullOrEmpty(s.Name))
                    ctx[s.Name] = s.Value ?? 0;
        if (sb?.SpecialStats != null)
            foreach (var s in sb.SpecialStats)
                if (!string.IsNullOrEmpty(s.Name))
                    ctx[s.Name] = s.Value ?? 0;
        if (gauges != null)
            foreach (var g in gauges)
                if (!string.IsNullOrEmpty(g.Name))
                {
                    ctx[g.Name] = g.Max;
                    ctx[g.Name + ".max"] = g.Max;
                }
        return ctx;
    }

    /// Evaluate an arithmetic expression with variable substitution, e.g. "dex/2+1".
    public static double? Evaluate(string expr, Dictionary<string, double> ctx)
    {
        if (string.IsNullOrWhiteSpace(expr)) return null;
        try
        {
            // Replace variable names with numeric values; longest names first to avoid partial matches.
            var resolved = ctx.Keys
                .OrderByDescending(k => k.Length)
                .Aggregate(expr, (e, key) => Regex.Replace(
                    e,
                    $@"(?<![a-zA-Z0-9_.]){Regex.Escape(key)}(?![a-zA-Z0-9_.])",
                    ctx[key].ToString(CultureInfo.InvariantCulture),
                    RegexOptions.IgnoreCase));

            return Convert.ToDouble(new DataTable().Compute(resolved, null));
        }
        catch { return null; }
    }

    /// Apply all matching buff formulae to baseValue and return the effective result.
    public static double ComputeEffective(
        IEnumerable<Buff> buffs,
        IEnumerable<string> targetNames,
        double baseValue,
        Dictionary<string, double> ctx)
    {
        var names = new HashSet<string>(targetNames, StringComparer.OrdinalIgnoreCase);
        var effective = baseValue;
        foreach (var buff in buffs)
            foreach (var formula in buff.Formulae)
            {
                var parsed = Parse(formula);
                if (parsed == null || !names.Contains(parsed.Value.target)) continue;
                var val = Evaluate(parsed.Value.expr, ctx);
                if (val == null) continue;
                effective = parsed.Value.op switch
                {
                    "+=" => effective + val.Value,
                    "-=" => effective - val.Value,
                    "*=" => effective * val.Value,
                    "/=" => val.Value != 0 ? effective / val.Value : effective,
                    _ => effective
                };
            }
        return effective;
    }

    /// Convenience overload for a single target name.
    public static double ComputeEffective(
        IEnumerable<Buff> buffs,
        string target,
        double baseValue,
        Dictionary<string, double> ctx)
        => ComputeEffective(buffs, [target], baseValue, ctx);

    /// Human-readable label, e.g. "STR + 1", "CON × dex/2".
    public static string FormatDisplay(string formula)
    {
        var parsed = Parse(formula);
        if (parsed == null) return formula;
        var (target, op, expr) = parsed.Value;
        var sym = op switch { "+=" => "+", "-=" => "-", "*=" => "×", "/=" => "÷", _ => op };
        return $"{target.ToUpper()} {sym} {expr}";
    }
}
