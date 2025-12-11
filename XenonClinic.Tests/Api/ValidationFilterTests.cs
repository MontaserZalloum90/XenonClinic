using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using XenonClinic.Api.Controllers;
using XenonClinic.Api.Filters;
using Xunit;

namespace XenonClinic.Tests.Api;

/// <summary>
/// Tests for the validation action filters.
/// </summary>
public class ValidationFilterTests
{
    #region ValidationActionFilter Tests

    [Fact]
    public void ValidationFilter_ValidModelState_AllowsExecution()
    {
        // Arrange
        var filter = new ValidationActionFilter();
        var context = CreateActionExecutingContext();

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void ValidationFilter_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var filter = new ValidationActionFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Name", "Name is required");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = (BadRequestObjectResult)context.Result!;
        var response = result.Value as ApiResponse;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
    }

    [Fact]
    public void ValidationFilter_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var filter = new ValidationActionFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Name", "Name is required");
        context.ModelState.AddModelError("Email", "Email is invalid");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = (BadRequestObjectResult)context.Result!;
        var response = result.Value as ApiResponse;
        response!.Errors.Should().NotBeNull();
        response.Errors.Should().HaveCount(2);
    }

    #endregion

    #region ValidatePaginationFilter Tests

    [Fact]
    public void PaginationFilter_ValidPagination_AllowsExecution()
    {
        // Arrange
        var filter = new ValidatePaginationFilter();
        var context = CreateActionExecutingContext();
        context.ActionArguments["pageNumber"] = 1;
        context.ActionArguments["pageSize"] = 20;

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void PaginationFilter_NegativePageNumber_ReturnsBadRequest()
    {
        // Arrange
        var filter = new ValidatePaginationFilter();
        var context = CreateActionExecutingContext();
        context.ActionArguments["pageNumber"] = -1;

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void PaginationFilter_ZeroPageNumber_ReturnsBadRequest()
    {
        // Arrange
        var filter = new ValidatePaginationFilter();
        var context = CreateActionExecutingContext();
        context.ActionArguments["pageNumber"] = 0;

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void PaginationFilter_ExcessivePageSize_ReturnsBadRequest()
    {
        // Arrange
        var filter = new ValidatePaginationFilter();
        var context = CreateActionExecutingContext();
        context.ActionArguments["pageSize"] = 1001;

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void PaginationFilter_NegativePageSize_ReturnsBadRequest()
    {
        // Arrange
        var filter = new ValidatePaginationFilter();
        var context = CreateActionExecutingContext();
        context.ActionArguments["pageSize"] = -10;

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void PaginationFilter_PaginationRequest_Validates()
    {
        // Arrange
        var filter = new ValidatePaginationFilter();
        var context = CreateActionExecutingContext();
        context.ActionArguments["request"] = new PaginationRequest { PageNumber = 1, PageSize = 50 };

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void PaginationFilter_InvalidPaginationRequest_ReturnsBadRequest()
    {
        // Arrange
        var filter = new ValidatePaginationFilter();
        var context = CreateActionExecutingContext();
        context.ActionArguments["request"] = new PaginationRequest { PageNumber = -1, PageSize = 50 };

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void PaginationFilter_MaxPageSize_IsValid()
    {
        // Arrange
        var filter = new ValidatePaginationFilter();
        var context = CreateActionExecutingContext();
        context.ActionArguments["pageSize"] = 1000;

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    #endregion

    #region ValidateModelAttribute Tests

    [Fact]
    public void ValidateModelAttribute_ValidModel_AllowsExecution()
    {
        // Arrange
        var filter = new ValidateModelAttribute();
        var context = CreateActionExecutingContext();

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void ValidateModelAttribute_NullArgument_ReturnsBadRequest()
    {
        // Arrange
        var filter = new ValidateModelAttribute();
        var context = CreateActionExecutingContext();
        context.ActionArguments["model"] = null;

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = (BadRequestObjectResult)context.Result!;
        var response = result.Value as ApiResponse;
        response!.Error.Should().Contain("model");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ValidationFilter_EmptyModelState_AllowsExecution()
    {
        // Arrange
        var filter = new ValidationActionFilter();
        var context = CreateActionExecutingContext();
        // ModelState is empty by default

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void PaginationFilter_NoPaginationParams_AllowsExecution()
    {
        // Arrange
        var filter = new ValidatePaginationFilter();
        var context = CreateActionExecutingContext();
        // No pagination arguments

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void ValidationFilter_FieldWithMultipleErrors_ReturnsAllFieldErrors()
    {
        // Arrange
        var filter = new ValidationActionFilter();
        var context = CreateActionExecutingContext();
        context.ModelState.AddModelError("Email", "Email is required");
        context.ModelState.AddModelError("Email", "Email format is invalid");

        // Act
        filter.OnActionExecuting(context);

        // Assert
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = (BadRequestObjectResult)context.Result!;
        var response = result.Value as ApiResponse;
        response!.Errors!["Email"].Should().HaveCount(2);
    }

    #endregion

    #region Helper Methods

    private static ActionExecutingContext CreateActionExecutingContext()
    {
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor();
        var modelState = new ModelStateDictionary();

        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor, modelState);
        var filters = new List<IFilterMetadata>();

        return new ActionExecutingContext(
            actionContext,
            filters,
            new Dictionary<string, object?>(),
            new object());
    }

    #endregion
}
