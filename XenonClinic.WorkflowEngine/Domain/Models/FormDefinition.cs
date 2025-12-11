namespace XenonClinic.WorkflowEngine.Domain.Models;

/// <summary>
/// Definition of a form for user input.
/// </summary>
public class FormDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// Form sections
    /// </summary>
    public List<FormSection> Sections { get; set; } = new();

    /// <summary>
    /// Global form settings
    /// </summary>
    public FormSettings? Settings { get; set; }

    /// <summary>
    /// JSON Schema for validation
    /// </summary>
    public Dictionary<string, object>? JsonSchema { get; set; }

    /// <summary>
    /// UI Schema hints
    /// </summary>
    public Dictionary<string, object>? UiSchema { get; set; }
}

public class FormSection
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }

    /// <summary>
    /// Expression to control visibility
    /// </summary>
    public string? VisibilityExpression { get; set; }

    /// <summary>
    /// CSS class for styling
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Columns in this section (1-4)
    /// </summary>
    public int Columns { get; set; } = 1;

    public List<FormField> Fields { get; set; } = new();
}

public class FormField
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Maps to process variable name
    /// </summary>
    public string VariableName { get; set; } = string.Empty;

    public FormFieldType Type { get; set; }

    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? Tooltip { get; set; }

    // Layout
    public int Order { get; set; }
    public int? ColSpan { get; set; }

    // Validation
    public bool IsRequired { get; set; }
    public string? RequiredExpression { get; set; }
    public string? ValidationExpression { get; set; }
    public string? ValidationMessage { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? Pattern { get; set; }

    // Behavior
    public bool IsReadOnly { get; set; }
    public string? ReadOnlyExpression { get; set; }
    public string? VisibilityExpression { get; set; }
    public string? DisabledExpression { get; set; }

    // Default value
    public object? DefaultValue { get; set; }
    public string? DefaultValueExpression { get; set; }

    // For dropdowns, radios, checkboxes
    public List<FormFieldOption>? Options { get; set; }
    public string? OptionsExpression { get; set; }

    // For lookups
    public LookupConfig? Lookup { get; set; }

    // For file uploads
    public FileUploadConfig? FileUpload { get; set; }

    // For tables (repeating rows)
    public TableConfig? Table { get; set; }

    // Styling
    public string? CssClass { get; set; }
    public string? Icon { get; set; }

    // Custom properties
    public Dictionary<string, object>? Properties { get; set; }
}

public enum FormFieldType
{
    Text,
    TextArea,
    RichText,
    Number,
    Currency,
    Percentage,
    Date,
    DateTime,
    Time,
    Checkbox,
    Switch,
    Radio,
    Dropdown,
    MultiSelect,
    Autocomplete,
    FileUpload,
    MultiFileUpload,
    Image,
    Signature,
    Table,
    UserPicker,
    GroupPicker,
    Lookup,
    Hidden,
    Label,
    Divider,
    Custom
}

public class FormFieldOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsDisabled { get; set; }
    public string? Group { get; set; }
}

public class LookupConfig
{
    /// <summary>
    /// Data source type: "api", "static", "process-variable"
    /// </summary>
    public string Source { get; set; } = "api";

    /// <summary>
    /// API endpoint for remote lookup
    /// </summary>
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// HTTP method
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Query parameter name for search term
    /// </summary>
    public string SearchParam { get; set; } = "q";

    /// <summary>
    /// Path to results array in response
    /// </summary>
    public string ResultsPath { get; set; } = "data";

    /// <summary>
    /// Field to use as value
    /// </summary>
    public string ValueField { get; set; } = "id";

    /// <summary>
    /// Field to display as label
    /// </summary>
    public string LabelField { get; set; } = "name";

    /// <summary>
    /// Minimum characters before searching
    /// </summary>
    public int MinSearchLength { get; set; } = 2;

    /// <summary>
    /// Debounce delay in ms
    /// </summary>
    public int DebounceMs { get; set; } = 300;

    /// <summary>
    /// Allow selecting multiple values
    /// </summary>
    public bool AllowMultiple { get; set; }

    /// <summary>
    /// Additional fields to include in selection
    /// </summary>
    public List<string>? IncludeFields { get; set; }
}

public class FileUploadConfig
{
    /// <summary>
    /// Allowed file extensions (e.g., [".pdf", ".doc"])
    /// </summary>
    public List<string>? AllowedExtensions { get; set; }

    /// <summary>
    /// Allowed MIME types
    /// </summary>
    public List<string>? AllowedMimeTypes { get; set; }

    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    public long? MaxSizeBytes { get; set; }

    /// <summary>
    /// Maximum number of files (for multi-upload)
    /// </summary>
    public int MaxFiles { get; set; } = 1;

    /// <summary>
    /// Upload endpoint
    /// </summary>
    public string? UploadEndpoint { get; set; }

    /// <summary>
    /// Show preview for images
    /// </summary>
    public bool ShowPreview { get; set; } = true;
}

public class TableConfig
{
    /// <summary>
    /// Column definitions
    /// </summary>
    public List<FormField> Columns { get; set; } = new();

    /// <summary>
    /// Minimum number of rows
    /// </summary>
    public int MinRows { get; set; }

    /// <summary>
    /// Maximum number of rows
    /// </summary>
    public int? MaxRows { get; set; }

    /// <summary>
    /// Allow adding rows
    /// </summary>
    public bool AllowAdd { get; set; } = true;

    /// <summary>
    /// Allow removing rows
    /// </summary>
    public bool AllowRemove { get; set; } = true;

    /// <summary>
    /// Allow reordering rows
    /// </summary>
    public bool AllowReorder { get; set; }

    /// <summary>
    /// Show row numbers
    /// </summary>
    public bool ShowRowNumbers { get; set; } = true;
}

public class FormSettings
{
    /// <summary>
    /// Layout mode: "vertical", "horizontal", "inline"
    /// </summary>
    public string Layout { get; set; } = "vertical";

    /// <summary>
    /// Label position: "top", "left", "floating"
    /// </summary>
    public string LabelPosition { get; set; } = "top";

    /// <summary>
    /// Show validation errors inline
    /// </summary>
    public bool ShowInlineErrors { get; set; } = true;

    /// <summary>
    /// Validate on blur
    /// </summary>
    public bool ValidateOnBlur { get; set; } = true;

    /// <summary>
    /// Validate on change
    /// </summary>
    public bool ValidateOnChange { get; set; }

    /// <summary>
    /// Submit button text
    /// </summary>
    public string? SubmitButtonText { get; set; }

    /// <summary>
    /// Cancel button text
    /// </summary>
    public string? CancelButtonText { get; set; }

    /// <summary>
    /// Show required indicator
    /// </summary>
    public bool ShowRequiredIndicator { get; set; } = true;

    /// <summary>
    /// Required indicator character
    /// </summary>
    public string RequiredIndicator { get; set; } = "*";

    /// <summary>
    /// Custom CSS
    /// </summary>
    public string? CustomCss { get; set; }
}
