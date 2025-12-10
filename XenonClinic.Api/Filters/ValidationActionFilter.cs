using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XenonClinic.Api.Controllers;

namespace XenonClinic.Api.Filters;

/// <summary>
/// Action filter that automatically handles model validation errors.
/// Returns standardized API error response when ModelState is invalid.
/// </summary>
public class ValidationActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => ToCamelCase(x.Key),
                    x => x.Value!.Errors.Select(e =>
                        string.IsNullOrEmpty(e.ErrorMessage)
                            ? "Invalid value"
                            : e.ErrorMessage).ToArray());

            var response = ApiResponse.Failure("Validation failed", errors);
            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No post-action processing needed
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}

/// <summary>
/// Action filter that validates pagination parameters.
/// </summary>
public class ValidatePaginationFilter : IActionFilter
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Find pagination parameters
        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value is PaginationRequest pagination)
            {
                // Enforce limits
                if (pagination.PageSize > MaxPageSize)
                    pagination.PageSize = MaxPageSize;
                if (pagination.PageSize < 1)
                    pagination.PageSize = DefaultPageSize;
                if (pagination.PageNumber < 1)
                    pagination.PageNumber = 1;
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No post-action processing needed
    }
}

/// <summary>
/// Attribute-based filter for custom validation rules.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            context.Result = new BadRequestObjectResult(ApiResponse.Failure("Validation failed", errors));
        }
    }
}

/// <summary>
/// Action filter that ensures required route parameters are present.
/// </summary>
public class RequiredRouteParameterFilter : IActionFilter
{
    private readonly string[] _requiredParameters;

    public RequiredRouteParameterFilter(params string[] requiredParameters)
    {
        _requiredParameters = requiredParameters;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var param in _requiredParameters)
        {
            if (!context.ActionArguments.ContainsKey(param) ||
                context.ActionArguments[param] == null)
            {
                context.Result = new BadRequestObjectResult(
                    ApiResponse.Failure($"Required parameter '{param}' is missing"));
                return;
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No post-action processing needed
    }
}
