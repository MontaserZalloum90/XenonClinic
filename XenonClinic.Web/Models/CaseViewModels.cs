using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Entities;

namespace XenonClinic.Web.Models;

/// <summary>
/// ViewModel for creating/editing a case
/// </summary>
public class CaseFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Case Number")]
    public string? CaseNumber { get; set; }

    [Required(ErrorMessage = "Patient is required")]
    [Display(Name = "Patient")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Branch is required")]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    [Required(ErrorMessage = "Case Type is required")]
    [Display(Name = "Case Type")]
    public int CaseTypeId { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [Display(Name = "Status")]
    public int CaseStatusId { get; set; }

    [Display(Name = "Assigned To")]
    public string? AssignedToUserId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [Display(Name = "Title")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Display(Name = "Priority")]
    public CasePriority Priority { get; set; } = CasePriority.Medium;

    [Display(Name = "Chief Complaint")]
    [DataType(DataType.MultilineText)]
    public string? ChiefComplaint { get; set; }

    [Display(Name = "Target Date")]
    [DataType(DataType.Date)]
    public DateTime? TargetDate { get; set; }

    [Display(Name = "Tags")]
    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    public string? Tags { get; set; }

    // For display purposes
    public string? PatientName { get; set; }
    public string? BranchName { get; set; }
}

/// <summary>
/// ViewModel for displaying a case in a list
/// </summary>
public class CaseListItemViewModel
{
    public int Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string CaseTypeName { get; set; } = string.Empty;
    public string CaseStatusName { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public CasePriority Priority { get; set; }
    public DateTime OpenedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public string? StatusColorCode { get; set; }
    public int PendingActivitiesCount { get; set; }
    public int NotesCount { get; set; }
}

/// <summary>
/// ViewModel for case details page
/// </summary>
public class CaseDetailsViewModel
{
    public Case Case { get; set; } = null!;
    public List<CaseNote> Notes { get; set; } = new();
    public List<CaseActivity> Activities { get; set; } = new();
    public List<CaseActivity> PendingActivities { get; set; } = new();
    public List<CaseActivity> OverdueActivities { get; set; } = new();
    public Dictionary<string, int> Statistics { get; set; } = new();

    // For adding new notes/activities
    public CaseNoteFormViewModel NewNote { get; set; } = new();
    public CaseActivityFormViewModel NewActivity { get; set; } = new();
}

/// <summary>
/// ViewModel for adding/editing a case note
/// </summary>
public class CaseNoteFormViewModel
{
    public int? Id { get; set; }

    [Required]
    public int CaseId { get; set; }

    [Required(ErrorMessage = "Note content is required")]
    [Display(Name = "Note")]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Note Type")]
    public CaseNoteType NoteType { get; set; } = CaseNoteType.General;

    [Display(Name = "Visible to Patient")]
    public bool IsVisibleToPatient { get; set; } = false;

    [Display(Name = "Pin this note")]
    public bool IsPinned { get; set; } = false;
}

/// <summary>
/// ViewModel for adding/editing a case activity
/// </summary>
public class CaseActivityFormViewModel
{
    public int? Id { get; set; }

    [Required]
    public int CaseId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [Display(Name = "Title")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Display(Name = "Activity Type")]
    public CaseActivityType ActivityType { get; set; } = CaseActivityType.Task;

    [Display(Name = "Status")]
    public CaseActivityStatus Status { get; set; } = CaseActivityStatus.Pending;

    [Display(Name = "Assigned To")]
    public string? AssignedToUserId { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Priority")]
    public CasePriority Priority { get; set; } = CasePriority.Medium;

    [Display(Name = "Result")]
    [DataType(DataType.MultilineText)]
    public string? Result { get; set; }
}

/// <summary>
/// ViewModel for case list/index page
/// </summary>
public class CaseListViewModel
{
    public List<CaseListItemViewModel> Cases { get; set; } = new();
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? StatusId { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? SearchTerm { get; set; }
    public bool IncludeClosed { get; set; } = false;

    // For filters
    public List<CaseTypeSelectItem> CaseTypes { get; set; } = new();
    public List<CaseStatusSelectItem> CaseStatuses { get; set; } = new();
    public Dictionary<string, int> Statistics { get; set; } = new();
}

/// <summary>
/// ViewModel for case status change
/// </summary>
public class ChangeCaseStatusViewModel
{
    [Required]
    public int CaseId { get; set; }

    [Required(ErrorMessage = "New status is required")]
    [Display(Name = "New Status")]
    public int NewStatusId { get; set; }

    [Display(Name = "Notes")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public List<CaseStatusSelectItem> AvailableStatuses { get; set; } = new();
}

/// <summary>
/// ViewModel for closing a case
/// </summary>
public class CloseCaseViewModel
{
    [Required]
    public int CaseId { get; set; }

    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Resolution is required")]
    [Display(Name = "Resolution")]
    [DataType(DataType.MultilineText)]
    public string Resolution { get; set; } = string.Empty;
}

// Select list items
public class CaseTypeSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public string? IconClass { get; set; }
}

public class CaseStatusSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public CaseStatusCategory Category { get; set; }
}

public class PatientSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmiratesId { get; set; } = string.Empty;
}

public class UserSelectItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
