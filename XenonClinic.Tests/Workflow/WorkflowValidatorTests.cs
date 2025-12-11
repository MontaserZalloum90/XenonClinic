using FluentAssertions;
using FluentValidation.TestHelper;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Validators;
using Xunit;

namespace XenonClinic.Tests.Workflow;

/// <summary>
/// Tests for workflow validators.
/// </summary>
public class WorkflowValidatorTests
{
    #region StartWorkflowRequestValidator Tests

    private readonly StartWorkflowRequestValidator _startValidator = new();

    [Fact]
    public void StartValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new StartWorkflowRequestDto
        {
            WorkflowId = "patient-intake-workflow",
            Name = "New Patient Intake",
            Priority = 10,
            Input = new Dictionary<string, object?> { ["patientId"] = 123 }
        };

        // Act
        var result = _startValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void StartValidator_EmptyWorkflowId_FailsValidation()
    {
        // Arrange
        var dto = new StartWorkflowRequestDto
        {
            WorkflowId = ""
        };

        // Act
        var result = _startValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkflowId);
    }

    [Fact]
    public void StartValidator_WorkflowIdTooLong_FailsValidation()
    {
        // Arrange
        var dto = new StartWorkflowRequestDto
        {
            WorkflowId = new string('x', 101)
        };

        // Act
        var result = _startValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WorkflowId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void StartValidator_InvalidPriority_FailsValidation(int priority)
    {
        // Arrange
        var dto = new StartWorkflowRequestDto
        {
            WorkflowId = "test-workflow",
            Priority = priority
        };

        // Act
        var result = _startValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void StartValidator_PastScheduledTime_FailsValidation()
    {
        // Arrange
        var dto = new StartWorkflowRequestDto
        {
            WorkflowId = "test-workflow",
            ScheduledStartTime = DateTime.UtcNow.AddHours(-1)
        };

        // Act
        var result = _startValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ScheduledStartTime);
    }

    [Fact]
    public void StartValidator_FutureScheduledTime_PassesValidation()
    {
        // Arrange
        var dto = new StartWorkflowRequestDto
        {
            WorkflowId = "test-workflow",
            ScheduledStartTime = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = _startValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ScheduledStartTime);
    }

    [Fact]
    public void StartValidator_InvalidVersion_FailsValidation()
    {
        // Arrange
        var dto = new StartWorkflowRequestDto
        {
            WorkflowId = "test-workflow",
            Version = 0
        };

        // Act
        var result = _startValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Version);
    }

    #endregion

    #region ResumeWorkflowRequestValidator Tests

    private readonly ResumeWorkflowRequestValidator _resumeValidator = new();

    [Fact]
    public void ResumeValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new ResumeWorkflowRequestDto
        {
            BookmarkName = "approval_required",
            Input = new Dictionary<string, object?> { ["approved"] = true }
        };

        // Act
        var result = _resumeValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ResumeValidator_EmptyBookmarkName_FailsValidation()
    {
        // Arrange
        var dto = new ResumeWorkflowRequestDto
        {
            BookmarkName = ""
        };

        // Act
        var result = _resumeValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BookmarkName);
    }

    [Fact]
    public void ResumeValidator_BookmarkNameTooLong_FailsValidation()
    {
        // Arrange
        var dto = new ResumeWorkflowRequestDto
        {
            BookmarkName = new string('x', 201)
        };

        // Act
        var result = _resumeValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BookmarkName);
    }

    #endregion

    #region SendSignalRequestValidator Tests

    private readonly SendSignalRequestValidator _signalValidator = new();

    [Fact]
    public void SignalValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new SendSignalRequestDto
        {
            SignalName = "approval_complete",
            Data = new { approved = true, comments = "Looks good" }
        };

        // Act
        var result = _signalValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SignalValidator_EmptySignalName_FailsValidation()
    {
        // Arrange
        var dto = new SendSignalRequestDto
        {
            SignalName = ""
        };

        // Act
        var result = _signalValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SignalName);
    }

    [Theory]
    [InlineData("123invalid")] // Starts with number
    [InlineData("invalid-name")] // Contains hyphen
    [InlineData("invalid name")] // Contains space
    public void SignalValidator_InvalidSignalNameFormat_FailsValidation(string signalName)
    {
        // Arrange
        var dto = new SendSignalRequestDto
        {
            SignalName = signalName
        };

        // Act
        var result = _signalValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SignalName);
    }

    [Theory]
    [InlineData("validSignal")]
    [InlineData("valid_signal")]
    [InlineData("Valid123")]
    [InlineData("A")]
    public void SignalValidator_ValidSignalNameFormat_PassesValidation(string signalName)
    {
        // Arrange
        var dto = new SendSignalRequestDto
        {
            SignalName = signalName
        };

        // Act
        var result = _signalValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SignalName);
    }

    #endregion

    #region TriggerEventRequestValidator Tests

    private readonly TriggerEventRequestValidator _eventValidator = new();

    [Fact]
    public void EventValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new TriggerEventRequestDto
        {
            EventName = "patient.registered",
            EventData = new { patientId = 123 }
        };

        // Act
        var result = _eventValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EventValidator_EmptyEventName_FailsValidation()
    {
        // Arrange
        var dto = new TriggerEventRequestDto
        {
            EventName = ""
        };

        // Act
        var result = _eventValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventName);
    }

    [Theory]
    [InlineData("patient.created")]
    [InlineData("appointment_scheduled")]
    [InlineData("Lab.Results.Ready")]
    public void EventValidator_ValidEventNameFormats_PassesValidation(string eventName)
    {
        // Arrange
        var dto = new TriggerEventRequestDto
        {
            EventName = eventName
        };

        // Act
        var result = _eventValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EventName);
    }

    #endregion

    #region WorkflowDefinitionListRequestValidator Tests

    private readonly WorkflowDefinitionListRequestValidator _defListValidator = new();

    [Fact]
    public void DefListValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new WorkflowDefinitionListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = "CreatedAt"
        };

        // Act
        var result = _defListValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DefListValidator_InvalidPageNumber_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowDefinitionListRequestDto
        {
            PageNumber = 0
        };

        // Act
        var result = _defListValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Fact]
    public void DefListValidator_PageSizeTooLarge_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowDefinitionListRequestDto
        {
            PageNumber = 1,
            PageSize = 101
        };

        // Act
        var result = _defListValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("CreatedAt")]
    [InlineData("UpdatedAt")]
    [InlineData("Category")]
    [InlineData("Version")]
    public void DefListValidator_ValidSortFields_PassesValidation(string sortBy)
    {
        // Arrange
        var dto = new WorkflowDefinitionListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = sortBy
        };

        // Act
        var result = _defListValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void DefListValidator_InvalidSortField_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowDefinitionListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = "InvalidField"
        };

        // Act
        var result = _defListValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    #endregion

    #region WorkflowInstanceListRequestValidator Tests

    private readonly WorkflowInstanceListRequestValidator _instListValidator = new();

    [Fact]
    public void InstListValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new WorkflowInstanceListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            Statuses = new List<string> { "Running", "Completed" }
        };

        // Act
        var result = _instListValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Running")]
    [InlineData("Suspended")]
    [InlineData("Completed")]
    [InlineData("Faulted")]
    [InlineData("Cancelled")]
    public void InstListValidator_ValidStatuses_PassesValidation(string status)
    {
        // Arrange
        var dto = new WorkflowInstanceListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            Statuses = new List<string> { status }
        };

        // Act
        var result = _instListValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Statuses);
    }

    [Fact]
    public void InstListValidator_InvalidStatus_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowInstanceListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            Statuses = new List<string> { "InvalidStatus" }
        };

        // Act
        var result = _instListValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Statuses);
    }

    [Fact]
    public void InstListValidator_InvalidDateRange_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowInstanceListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            CreatedAfter = DateTime.UtcNow,
            CreatedBefore = DateTime.UtcNow.AddDays(-7) // Before is before After
        };

        // Act
        var result = _instListValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreatedAfter);
    }

    [Theory]
    [InlineData("CreatedAt")]
    [InlineData("StartedAt")]
    [InlineData("CompletedAt")]
    [InlineData("Status")]
    [InlineData("Priority")]
    public void InstListValidator_ValidSortFields_PassesValidation(string sortBy)
    {
        // Arrange
        var dto = new WorkflowInstanceListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = sortBy
        };

        // Act
        var result = _instListValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    #endregion

    #region WorkflowParameterValidator Tests

    private readonly WorkflowParameterValidator _paramValidator = new();

    [Fact]
    public void ParamValidator_ValidParameter_PassesValidation()
    {
        // Arrange
        var dto = new WorkflowParameterDto
        {
            Name = "patientId",
            Type = "integer",
            Description = "The patient's unique identifier",
            IsRequired = true
        };

        // Act
        var result = _paramValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ParamValidator_EmptyName_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowParameterDto
        {
            Name = "",
            Type = "string"
        };

        // Act
        var result = _paramValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("123param")] // Starts with number
    [InlineData("param-name")] // Contains hyphen
    public void ParamValidator_InvalidNameFormat_FailsValidation(string name)
    {
        // Arrange
        var dto = new WorkflowParameterDto
        {
            Name = name,
            Type = "string"
        };

        // Act
        var result = _paramValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("string")]
    [InlineData("number")]
    [InlineData("integer")]
    [InlineData("boolean")]
    [InlineData("date")]
    [InlineData("datetime")]
    [InlineData("object")]
    [InlineData("array")]
    public void ParamValidator_ValidTypes_PassesValidation(string type)
    {
        // Arrange
        var dto = new WorkflowParameterDto
        {
            Name = "testParam",
            Type = type
        };

        // Act
        var result = _paramValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void ParamValidator_InvalidType_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowParameterDto
        {
            Name = "testParam",
            Type = "invalidType"
        };

        // Act
        var result = _paramValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    #endregion

    #region WorkflowTriggerValidator Tests

    private readonly WorkflowTriggerValidator _triggerValidator = new();

    [Fact]
    public void TriggerValidator_ValidManualTrigger_PassesValidation()
    {
        // Arrange
        var dto = new WorkflowTriggerDto
        {
            Name = "Manual Start",
            Type = "Manual",
            IsEnabled = true,
            Config = new Dictionary<string, object?>()
        };

        // Act
        var result = _triggerValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TriggerValidator_ValidScheduledTrigger_PassesValidation()
    {
        // Arrange
        var dto = new WorkflowTriggerDto
        {
            Name = "Daily Run",
            Type = "Scheduled",
            IsEnabled = true,
            Config = new Dictionary<string, object?> { ["cron"] = "0 0 * * *" }
        };

        // Act
        var result = _triggerValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TriggerValidator_ScheduledWithoutCron_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowTriggerDto
        {
            Name = "Daily Run",
            Type = "Scheduled",
            IsEnabled = true,
            Config = new Dictionary<string, object?>() // Missing cron
        };

        // Act
        var result = _triggerValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Config);
    }

    [Fact]
    public void TriggerValidator_ValidEventTrigger_PassesValidation()
    {
        // Arrange
        var dto = new WorkflowTriggerDto
        {
            Name = "Patient Registered",
            Type = "Event",
            IsEnabled = true,
            Config = new Dictionary<string, object?> { ["eventName"] = "patient.registered" }
        };

        // Act
        var result = _triggerValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TriggerValidator_EventWithoutEventName_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowTriggerDto
        {
            Name = "Patient Event",
            Type = "Event",
            IsEnabled = true,
            Config = new Dictionary<string, object?>() // Missing eventName
        };

        // Act
        var result = _triggerValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Config);
    }

    [Fact]
    public void TriggerValidator_ValidWebhookTrigger_PassesValidation()
    {
        // Arrange
        var dto = new WorkflowTriggerDto
        {
            Name = "External Integration",
            Type = "Webhook",
            IsEnabled = true,
            Config = new Dictionary<string, object?> { ["path"] = "/webhooks/lab-results" }
        };

        // Act
        var result = _triggerValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TriggerValidator_EmptyName_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowTriggerDto
        {
            Name = "",
            Type = "Manual"
        };

        // Act
        var result = _triggerValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void TriggerValidator_InvalidType_FailsValidation()
    {
        // Arrange
        var dto = new WorkflowTriggerDto
        {
            Name = "Test",
            Type = "InvalidType"
        };

        // Act
        var result = _triggerValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    #endregion
}
