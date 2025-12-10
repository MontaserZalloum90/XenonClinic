namespace XenonClinic.WorkflowEngine.Tests.Activities;

using FluentAssertions;
using Moq;
using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Core.Activities;
using Xunit;

public class ConditionEvaluatorTests
{
    private readonly Mock<IWorkflowContext> _contextMock;

    public ConditionEvaluatorTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>());
        _contextMock.Setup(c => c.Input).Returns(new Dictionary<string, object?>());
    }

    private object? SimpleResolver(string expression, IWorkflowContext context)
    {
        // Remove quotes
        var expr = expression.Trim().Trim('\'', '"');

        // Check for numeric literal
        if (double.TryParse(expr, out var num))
            return num;
        if (int.TryParse(expr, out var intVal))
            return intVal;
        if (bool.TryParse(expr, out var boolVal))
            return boolVal;

        // Check variables
        if (expr.StartsWith("var."))
        {
            var varName = expr.Substring(4);
            return context.Variables.TryGetValue(varName, out var val) ? val : null;
        }

        // Check input
        if (expr.StartsWith("input."))
        {
            var inputName = expr.Substring(6);
            return context.Input.TryGetValue(inputName, out var val) ? val : null;
        }

        return expr;
    }

    [Theory]
    [InlineData("10 == 10", true)]
    [InlineData("10 == 20", false)]
    [InlineData("10 != 20", true)]
    [InlineData("10 != 10", false)]
    public void Evaluate_EqualityOperators_ReturnsCorrectResult(string condition, bool expected)
    {
        var result = ConditionEvaluator.Evaluate(condition, _contextMock.Object, SimpleResolver);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("10 > 5", true)]
    [InlineData("5 > 10", false)]
    [InlineData("10 > 10", false)]
    [InlineData("10 < 20", true)]
    [InlineData("20 < 10", false)]
    [InlineData("10 < 10", false)]
    public void Evaluate_GreaterLessThan_ReturnsCorrectResult(string condition, bool expected)
    {
        var result = ConditionEvaluator.Evaluate(condition, _contextMock.Object, SimpleResolver);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("10 >= 10", true)]
    [InlineData("10 >= 5", true)]
    [InlineData("5 >= 10", false)]
    [InlineData("10 <= 10", true)]
    [InlineData("5 <= 10", true)]
    [InlineData("10 <= 5", false)]
    public void Evaluate_GreaterLessThanOrEqual_ReturnsCorrectResult(string condition, bool expected)
    {
        var result = ConditionEvaluator.Evaluate(condition, _contextMock.Object, SimpleResolver);
        result.Should().Be(expected);
    }

    [Fact]
    public void Evaluate_WithVariables_ReturnsCorrectResult()
    {
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>
        {
            ["amount"] = 150
        });

        var result = ConditionEvaluator.Evaluate("var.amount > 100", _contextMock.Object, SimpleResolver);
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WithInput_ReturnsCorrectResult()
    {
        _contextMock.Setup(c => c.Input).Returns(new Dictionary<string, object?>
        {
            ["status"] = "approved"
        });

        var result = ConditionEvaluator.Evaluate("input.status == 'approved'", _contextMock.Object, SimpleResolver);
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_StringComparison_CaseInsensitive()
    {
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>
        {
            ["status"] = "APPROVED"
        });

        var result = ConditionEvaluator.Evaluate("var.status == 'approved'", _contextMock.Object, SimpleResolver);
        // Note: exact behavior depends on how string comparison is handled
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Evaluate_EmptyOrNullCondition_ReturnsFalse(string? condition)
    {
        var result = ConditionEvaluator.Evaluate(condition!, _contextMock.Object, SimpleResolver);
        result.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_BooleanExpression_ReturnsCorrectResult()
    {
        var result = ConditionEvaluator.Evaluate("true", _contextMock.Object, SimpleResolver);
        result.Should().BeTrue();

        result = ConditionEvaluator.Evaluate("false", _contextMock.Object, SimpleResolver);
        result.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_OperatorOrder_GreaterEqualBeforeGreater()
    {
        // This test ensures >= is evaluated before > to prevent incorrect parsing
        // e.g., "10 >= 5" should not be parsed as "10 >" + "= 5"
        var result = ConditionEvaluator.Evaluate("10 >= 5", _contextMock.Object, SimpleResolver);
        result.Should().BeTrue();

        result = ConditionEvaluator.Evaluate("5 >= 10", _contextMock.Object, SimpleResolver);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("100 >= 100", true)]
    [InlineData("101 >= 100", true)]
    [InlineData("99 >= 100", false)]
    public void Evaluate_BoundaryValues_ReturnsCorrectResult(string condition, bool expected)
    {
        var result = ConditionEvaluator.Evaluate(condition, _contextMock.Object, SimpleResolver);
        result.Should().Be(expected);
    }
}
