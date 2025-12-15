namespace XenonClinic.Core.Enums;

/// <summary>
/// Employment status for employees
/// </summary>
public enum EmploymentStatus
{
    Active = 0,
    OnProbation = 1,
    OnLeave = 2,
    Suspended = 3,
    Resigned = 4,
    Terminated = 5,
    Retired = 6
}

/// <summary>
/// Leave request status
/// </summary>
public enum LeaveRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

/// <summary>
/// Types of leave
/// </summary>
public enum LeaveType
{
    Annual = 0,
    Sick = 1,
    Maternity = 2,
    Paternity = 3,
    Bereavement = 4,
    Unpaid = 5,
    Emergency = 6,
    Study = 7,
    Other = 8
}
