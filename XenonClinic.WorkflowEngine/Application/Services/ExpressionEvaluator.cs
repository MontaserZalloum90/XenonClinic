namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Expression evaluator using System.Linq.Dynamic.Core.
/// Supports:
/// - Variable substitution: ${variableName}
/// - Property access: ${object.property}
/// - Boolean expressions: ${amount > 1000}
/// - Method calls: ${list.Count()}
/// </summary>
public partial class ExpressionEvaluator : IExpressionEvaluator
{
    private readonly ILogger<ExpressionEvaluator> _logger;
    private static readonly Regex VariablePattern = MyRegex();

    public ExpressionEvaluator(ILogger<ExpressionEvaluator> logger)
    {
        _logger = logger;
    }

    public async Task<object> EvaluateAsync(string expression, IDictionary<string, object> context)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return expression;

        try
        {
            // Check if it's a simple variable reference
            var simpleVarMatch = Regex.Match(expression.Trim(), @"^\$\{([^}]+)\}$");
            if (simpleVarMatch.Success)
            {
                var varName = simpleVarMatch.Groups[1].Value;
                return GetValueFromPath(varName, context);
            }

            // Check if it's a FEEL-style expression (#{...})
            var feelMatch = Regex.Match(expression.Trim(), @"^#\{([^}]+)\}$");
            if (feelMatch.Success)
            {
                var expr = feelMatch.Groups[1].Value;
                return await EvaluateDynamicExpressionAsync(expr, context);
            }

            // Check if it contains ${} patterns for substitution
            if (expression.Contains("${"))
            {
                return SubstituteVariables(expression, context);
            }

            // Try to evaluate as a dynamic expression
            return await EvaluateDynamicExpressionAsync(expression, context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Expression evaluation failed: {Expression}", expression);
            return expression;
        }
    }

    public async Task<T> EvaluateAsync<T>(string expression, IDictionary<string, object> context)
    {
        var result = await EvaluateAsync(expression, context);

        if (result is T typedResult)
            return typedResult;

        try
        {
            return (T)Convert.ChangeType(result, typeof(T));
        }
        catch
        {
            return default!;
        }
    }

    public async Task<bool> EvaluateConditionAsync(string expression, IDictionary<string, object> context)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return true;

        try
        {
            var result = await EvaluateAsync(expression, context);

            return result switch
            {
                bool b => b,
                string s => !string.IsNullOrEmpty(s) && !s.Equals("false", StringComparison.OrdinalIgnoreCase),
                int i => i != 0,
                decimal d => d != 0,
                double dbl => dbl != 0,
                null => false,
                _ => true
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Condition evaluation failed: {Expression}", expression);
            return false;
        }
    }

    public ExpressionValidationResult Validate(string expression)
    {
        var result = new ExpressionValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(expression))
            return result;

        try
        {
            // Extract all variable references
            var matches = VariablePattern.Matches(expression);
            foreach (Match match in matches)
            {
                var varPath = match.Groups[1].Value;
                var rootVar = varPath.Split('.')[0];
                if (!result.Variables.Contains(rootVar))
                {
                    result.Variables.Add(rootVar);
                }
            }

            // Try to parse as a dynamic expression (with dummy context)
            var cleanExpression = VariablePattern.Replace(expression, "true");
            if (!string.IsNullOrWhiteSpace(cleanExpression) && cleanExpression != expression)
            {
                try
                {
                    DynamicExpressionParser.ParseLambda(
                        Array.Empty<System.Linq.Expressions.ParameterExpression>(),
                        typeof(object),
                        cleanExpression);
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Error = ex.Message;
                }
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Error = ex.Message;
        }

        return result;
    }

    private async Task<object> EvaluateDynamicExpressionAsync(string expression, IDictionary<string, object> context)
    {
        // Replace variable references with parameter access
        var parameters = new List<System.Linq.Expressions.ParameterExpression>();
        var parameterValues = new List<object>();
        var modifiedExpression = expression;

        // Create a wrapper object with all context variables
        var contextWrapper = new DynamicContext(context);

        try
        {
            // Parse and execute the lambda
            var lambda = DynamicExpressionParser.ParseLambda(
                new[] { System.Linq.Expressions.Expression.Parameter(typeof(DynamicContext), "ctx") },
                typeof(object),
                TransformExpression(expression));

            var compiled = lambda.Compile();
            var result = compiled.DynamicInvoke(contextWrapper);

            return await Task.FromResult(result ?? expression);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Dynamic expression evaluation failed for: {Expression}", expression);

            // Fall back to simple substitution
            return SubstituteVariables(expression, context);
        }
    }

    private static string TransformExpression(string expression)
    {
        // Transform ${varName} to ctx.Get("varName")
        // Transform variable.property to ctx.Get("variable.property")
        var transformed = VariablePattern.Replace(expression, match =>
        {
            var path = match.Groups[1].Value;
            return $"ctx.Get(\"{path}\")";
        });

        // If no ${} found, try to handle bare variable names in comparisons
        if (transformed == expression && !expression.Contains("ctx.Get"))
        {
            // Simple transformation for bare comparisons like "amount > 1000"
            var comparisonMatch = Regex.Match(expression, @"^(\w+)\s*(==|!=|>|<|>=|<=)\s*(.+)$");
            if (comparisonMatch.Success)
            {
                var varName = comparisonMatch.Groups[1].Value;
                var op = comparisonMatch.Groups[2].Value;
                var value = comparisonMatch.Groups[3].Value;
                return $"ctx.GetNumber(\"{varName}\") {op} {value}";
            }
        }

        return transformed;
    }

    private static string SubstituteVariables(string template, IDictionary<string, object> context)
    {
        return VariablePattern.Replace(template, match =>
        {
            var path = match.Groups[1].Value;
            var value = GetValueFromPath(path, context);
            return value?.ToString() ?? "";
        });
    }

    private static object GetValueFromPath(string path, IDictionary<string, object> context)
    {
        var parts = path.Split('.');
        object? current = null;

        // First part is the root variable
        if (context.TryGetValue(parts[0], out var rootValue))
        {
            current = rootValue;
        }
        else
        {
            return path; // Return the path as-is if variable not found
        }

        // Navigate nested properties
        for (var i = 1; i < parts.Length && current != null; i++)
        {
            var prop = current.GetType().GetProperty(parts[i]);
            if (prop != null)
            {
                current = prop.GetValue(current);
            }
            else if (current is IDictionary<string, object> dict)
            {
                dict.TryGetValue(parts[i], out current);
            }
            else
            {
                return path;
            }
        }

        return current ?? path;
    }

    [GeneratedRegex(@"\$\{([^}]+)\}|#\{([^}]+)\}")]
    private static partial Regex MyRegex();
}

/// <summary>
/// Dynamic context wrapper for expression evaluation.
/// </summary>
public class DynamicContext
{
    private readonly IDictionary<string, object> _data;

    public DynamicContext(IDictionary<string, object> data)
    {
        _data = data;
    }

    public object Get(string path)
    {
        var parts = path.Split('.');
        object? current = null;

        if (_data.TryGetValue(parts[0], out var rootValue))
        {
            current = rootValue;
        }
        else
        {
            return null!;
        }

        for (var i = 1; i < parts.Length && current != null; i++)
        {
            var prop = current.GetType().GetProperty(parts[i]);
            if (prop != null)
            {
                current = prop.GetValue(current);
            }
            else if (current is IDictionary<string, object> dict)
            {
                dict.TryGetValue(parts[i], out current);
            }
            else
            {
                return null!;
            }
        }

        return current!;
    }

    public decimal GetNumber(string path)
    {
        var value = Get(path);
        return value switch
        {
            decimal d => d,
            int i => i,
            long l => l,
            double dbl => (decimal)dbl,
            float f => (decimal)f,
            string s when decimal.TryParse(s, out var parsed) => parsed,
            _ => 0
        };
    }

    public string GetString(string path)
    {
        var value = Get(path);
        return value?.ToString() ?? "";
    }

    public bool GetBool(string path)
    {
        var value = Get(path);
        return value switch
        {
            bool b => b,
            string s => s.Equals("true", StringComparison.OrdinalIgnoreCase),
            int i => i != 0,
            _ => false
        };
    }
}
