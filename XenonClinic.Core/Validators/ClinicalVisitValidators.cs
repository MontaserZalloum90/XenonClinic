using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Validators;

#region Base Validators

/// <summary>
/// Base validator for all clinical visit creation DTOs.
/// </summary>
public abstract class CreateClinicalVisitBaseValidator<T> : AbstractValidator<T>
    where T : CreateClinicalVisitBaseDto
{
    protected CreateClinicalVisitBaseValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(ClinicalVisitValidationMessages.PatientIdRequired);

        RuleFor(x => x.VisitDate)
            .NotEmpty().WithMessage(ClinicalVisitValidationMessages.VisitDateRequired)
            .Must(NotBeInFuture).WithMessage(ClinicalVisitValidationMessages.VisitDateInFuture);

        RuleFor(x => x.ChiefComplaint)
            .NotEmpty().WithMessage(ClinicalVisitValidationMessages.ChiefComplaintRequired)
            .MaximumLength(500).WithMessage("Chief complaint cannot exceed 500 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(4000).WithMessage("Notes cannot exceed 4000 characters");

        RuleFor(x => x.ProviderId)
            .GreaterThan(0).WithMessage(ClinicalVisitValidationMessages.ProviderNotFound)
            .When(x => x.ProviderId.HasValue);
    }

    private static bool NotBeInFuture(DateTime visitDate)
    {
        return visitDate.Date <= DateTime.UtcNow.Date;
    }
}

/// <summary>
/// Base validator for all clinical visit update DTOs.
/// </summary>
public abstract class UpdateClinicalVisitBaseValidator<T> : AbstractValidator<T>
    where T : UpdateClinicalVisitBaseDto
{
    protected UpdateClinicalVisitBaseValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid visit ID");

        RuleFor(x => x.ChiefComplaint)
            .MaximumLength(500).WithMessage("Chief complaint cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ChiefComplaint));

        RuleFor(x => x.Notes)
            .MaximumLength(4000).WithMessage("Notes cannot exceed 4000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.ProviderId)
            .GreaterThan(0).WithMessage(ClinicalVisitValidationMessages.ProviderNotFound)
            .When(x => x.ProviderId.HasValue);
    }
}

#endregion

#region Audiology Validators

/// <summary>
/// Validator for CreateAudiologyVisitDto.
/// </summary>
public class CreateAudiologyVisitValidator : CreateClinicalVisitBaseValidator<CreateAudiologyVisitDto>
{
    private static readonly string[] ValidHearingLossTypes =
    {
        "Conductive", "Sensorineural", "Mixed", "Neural", "Functional"
    };

    private static readonly string[] ValidHearingAidTypes =
    {
        "BTE", "ITE", "ITC", "CIC", "RIC", "CROS", "BiCROS", "Bone-Anchored"
    };

    public CreateAudiologyVisitValidator()
    {
        RuleFor(x => x.HearingLossType)
            .Must(BeValidHearingLossType)
            .WithMessage("Invalid hearing loss type")
            .When(x => !string.IsNullOrEmpty(x.HearingLossType));

        RuleFor(x => x.AudiogramResults)
            .MaximumLength(2000).WithMessage("Audiogram results cannot exceed 2000 characters");

        RuleFor(x => x.Recommendations)
            .MaximumLength(2000).WithMessage("Recommendations cannot exceed 2000 characters");

        RuleFor(x => x.HearingAidType)
            .Must(BeValidHearingAidType)
            .WithMessage("Invalid hearing aid type")
            .When(x => !string.IsNullOrEmpty(x.HearingAidType));

        // If hearing aid recommended, type should ideally be specified
        RuleFor(x => x.HearingAidType)
            .NotEmpty()
            .WithMessage("Hearing aid type should be specified when recommended")
            .When(x => x.HearingAidRecommended == true);
    }

    private static bool BeValidHearingLossType(string? type)
    {
        return string.IsNullOrEmpty(type) ||
               ValidHearingLossTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidHearingAidType(string? type)
    {
        return string.IsNullOrEmpty(type) ||
               ValidHearingAidTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Validator for UpdateAudiologyVisitDto.
/// </summary>
public class UpdateAudiologyVisitValidator : UpdateClinicalVisitBaseValidator<UpdateAudiologyVisitDto>
{
    private static readonly string[] ValidHearingLossTypes =
    {
        "Conductive", "Sensorineural", "Mixed", "Neural", "Functional"
    };

    public UpdateAudiologyVisitValidator()
    {
        RuleFor(x => x.HearingLossType)
            .Must(type => string.IsNullOrEmpty(type) ||
                         ValidHearingLossTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid hearing loss type")
            .When(x => !string.IsNullOrEmpty(x.HearingLossType));

        RuleFor(x => x.AudiogramResults)
            .MaximumLength(2000).WithMessage("Audiogram results cannot exceed 2000 characters");

        RuleFor(x => x.Recommendations)
            .MaximumLength(2000).WithMessage("Recommendations cannot exceed 2000 characters");
    }
}

/// <summary>
/// Validator for AudiogramDto.
/// </summary>
public class AudiogramValidator : AbstractValidator<AudiogramDto>
{
    private static readonly string[] ValidTestTypes =
    {
        "Pure Tone", "Speech", "Tympanometry", "OAE", "ABR", "ASSR"
    };

    public AudiogramValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(ClinicalVisitValidationMessages.PatientIdRequired);

        RuleFor(x => x.TestDate)
            .NotEmpty().WithMessage("Test date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Test date cannot be in the future");

        RuleFor(x => x.TestType)
            .Must(type => string.IsNullOrEmpty(type) ||
                         ValidTestTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid test type");

        // Threshold validations (typical range -10 to 120 dB)
        RuleFor(x => x.Right250Hz).InclusiveBetween(-10, 120)
            .WithMessage("Right 250Hz threshold must be between -10 and 120 dB")
            .When(x => x.Right250Hz.HasValue);

        RuleFor(x => x.Right500Hz).InclusiveBetween(-10, 120)
            .WithMessage("Right 500Hz threshold must be between -10 and 120 dB")
            .When(x => x.Right500Hz.HasValue);

        RuleFor(x => x.Right1000Hz).InclusiveBetween(-10, 120)
            .WithMessage("Right 1000Hz threshold must be between -10 and 120 dB")
            .When(x => x.Right1000Hz.HasValue);

        RuleFor(x => x.Right2000Hz).InclusiveBetween(-10, 120)
            .WithMessage("Right 2000Hz threshold must be between -10 and 120 dB")
            .When(x => x.Right2000Hz.HasValue);

        RuleFor(x => x.Right4000Hz).InclusiveBetween(-10, 120)
            .WithMessage("Right 4000Hz threshold must be between -10 and 120 dB")
            .When(x => x.Right4000Hz.HasValue);

        RuleFor(x => x.Right8000Hz).InclusiveBetween(-10, 120)
            .WithMessage("Right 8000Hz threshold must be between -10 and 120 dB")
            .When(x => x.Right8000Hz.HasValue);

        RuleFor(x => x.Left250Hz).InclusiveBetween(-10, 120)
            .WithMessage("Left 250Hz threshold must be between -10 and 120 dB")
            .When(x => x.Left250Hz.HasValue);

        RuleFor(x => x.Left500Hz).InclusiveBetween(-10, 120)
            .WithMessage("Left 500Hz threshold must be between -10 and 120 dB")
            .When(x => x.Left500Hz.HasValue);

        RuleFor(x => x.Left1000Hz).InclusiveBetween(-10, 120)
            .WithMessage("Left 1000Hz threshold must be between -10 and 120 dB")
            .When(x => x.Left1000Hz.HasValue);

        RuleFor(x => x.Left2000Hz).InclusiveBetween(-10, 120)
            .WithMessage("Left 2000Hz threshold must be between -10 and 120 dB")
            .When(x => x.Left2000Hz.HasValue);

        RuleFor(x => x.Left4000Hz).InclusiveBetween(-10, 120)
            .WithMessage("Left 4000Hz threshold must be between -10 and 120 dB")
            .When(x => x.Left4000Hz.HasValue);

        RuleFor(x => x.Left8000Hz).InclusiveBetween(-10, 120)
            .WithMessage("Left 8000Hz threshold must be between -10 and 120 dB")
            .When(x => x.Left8000Hz.HasValue);

        // Speech recognition (0-100%)
        RuleFor(x => x.RightSpeechRecognition).InclusiveBetween(0, 100)
            .WithMessage("Right speech recognition must be between 0 and 100%")
            .When(x => x.RightSpeechRecognition.HasValue);

        RuleFor(x => x.LeftSpeechRecognition).InclusiveBetween(0, 100)
            .WithMessage("Left speech recognition must be between 0 and 100%")
            .When(x => x.LeftSpeechRecognition.HasValue);

        RuleFor(x => x.Interpretation)
            .MaximumLength(2000).WithMessage("Interpretation cannot exceed 2000 characters");
    }
}

#endregion

#region Dental Validators

/// <summary>
/// Validator for CreateDentalVisitDto.
/// </summary>
public class CreateDentalVisitValidator : CreateClinicalVisitBaseValidator<CreateDentalVisitDto>
{
    public CreateDentalVisitValidator()
    {
        RuleFor(x => x.ExaminationFindings)
            .MaximumLength(4000).WithMessage("Examination findings cannot exceed 4000 characters");

        RuleFor(x => x.Diagnosis)
            .MaximumLength(2000).WithMessage("Diagnosis cannot exceed 2000 characters");

        RuleFor(x => x.TreatmentProvided)
            .MaximumLength(4000).WithMessage("Treatment provided cannot exceed 4000 characters");

        RuleFor(x => x.ProcedureIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("All procedure IDs must be valid");
    }
}

/// <summary>
/// Validator for DentalProcedureDto.
/// </summary>
public class DentalProcedureValidator : AbstractValidator<DentalProcedureDto>
{
    private static readonly string[] ValidToothNumbers = GenerateToothNumbers();
    private static readonly string[] ValidSurfaces = { "M", "O", "D", "B", "L", "I", "F", "MOD", "MO", "DO", "MODBL" };
    private static readonly string[] ValidStatuses = { "Planned", "InProgress", "Completed", "Cancelled" };

    public DentalProcedureValidator()
    {
        RuleFor(x => x.VisitId)
            .GreaterThan(0).WithMessage("Invalid visit ID");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Procedure code is required")
            .MaximumLength(20).WithMessage("Procedure code cannot exceed 20 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Procedure name is required")
            .MaximumLength(200).WithMessage("Procedure name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.ToothNumber)
            .Must(BeValidToothNumber)
            .WithMessage("Invalid tooth number (use FDI notation: 11-18, 21-28, 31-38, 41-48)")
            .When(x => !string.IsNullOrEmpty(x.ToothNumber));

        RuleFor(x => x.Surface)
            .Must(surface => string.IsNullOrEmpty(surface) ||
                            ValidSurfaces.Contains(surface.ToUpperInvariant()))
            .WithMessage("Invalid surface designation")
            .When(x => !string.IsNullOrEmpty(x.Surface));

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost cannot be negative");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) ||
                           ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid status")
            .When(x => !string.IsNullOrEmpty(x.Status));
    }

    private static string[] GenerateToothNumbers()
    {
        var teeth = new List<string>();
        // FDI notation: quadrant (1-4) + tooth (1-8)
        for (int quadrant = 1; quadrant <= 4; quadrant++)
        {
            for (int tooth = 1; tooth <= 8; tooth++)
            {
                teeth.Add($"{quadrant}{tooth}");
            }
        }
        // Also add primary teeth (5-8 quadrants)
        for (int quadrant = 5; quadrant <= 8; quadrant++)
        {
            for (int tooth = 1; tooth <= 5; tooth++)
            {
                teeth.Add($"{quadrant}{tooth}");
            }
        }
        return teeth.ToArray();
    }

    private static bool BeValidToothNumber(string? toothNumber)
    {
        return string.IsNullOrEmpty(toothNumber) ||
               ValidToothNumbers.Contains(toothNumber);
    }
}

/// <summary>
/// Validator for ToothRecordDto.
/// </summary>
public class ToothRecordValidator : AbstractValidator<ToothRecordDto>
{
    private static readonly string[] ValidConditions =
    {
        "Healthy", "Decayed", "Filled", "Crown", "Missing", "Impacted",
        "Root Canal", "Bridge Abutment", "Implant", "Veneer"
    };

    public ToothRecordValidator()
    {
        RuleFor(x => x.ToothNumber)
            .NotEmpty().WithMessage("Tooth number is required")
            .MaximumLength(5).WithMessage("Invalid tooth number");

        RuleFor(x => x.Condition)
            .Must(condition => string.IsNullOrEmpty(condition) ||
                              ValidConditions.Contains(condition, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid tooth condition")
            .When(x => !string.IsNullOrEmpty(x.Condition));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
    }
}

#endregion

#region Cardiology Validators

/// <summary>
/// Validator for CreateCardioVisitDto.
/// </summary>
public class CreateCardioVisitValidator : CreateClinicalVisitBaseValidator<CreateCardioVisitDto>
{
    private static readonly string[] ValidRhythmAssessments =
    {
        "Regular", "Irregular", "Regularly Irregular", "Irregularly Irregular",
        "Sinus Rhythm", "Atrial Fibrillation", "Atrial Flutter", "SVT", "VT"
    };

    public CreateCardioVisitValidator()
    {
        // Blood pressure validation
        RuleFor(x => x.BloodPressureSystolic)
            .InclusiveBetween(60, 300)
            .WithMessage("Systolic blood pressure must be between 60 and 300 mmHg")
            .When(x => x.BloodPressureSystolic.HasValue);

        RuleFor(x => x.BloodPressureDiastolic)
            .InclusiveBetween(30, 200)
            .WithMessage("Diastolic blood pressure must be between 30 and 200 mmHg")
            .When(x => x.BloodPressureDiastolic.HasValue);

        RuleFor(x => x)
            .Must(HaveValidBloodPressure)
            .WithMessage("Systolic pressure must be greater than diastolic pressure")
            .When(x => x.BloodPressureSystolic.HasValue && x.BloodPressureDiastolic.HasValue);

        // Heart rate validation (reasonable range 20-300 bpm)
        RuleFor(x => x.HeartRate)
            .InclusiveBetween(20, 300)
            .WithMessage("Heart rate must be between 20 and 300 bpm")
            .When(x => x.HeartRate.HasValue);

        RuleFor(x => x.HeartSoundFindings)
            .MaximumLength(2000).WithMessage("Heart sound findings cannot exceed 2000 characters");

        RuleFor(x => x.RhythmAssessment)
            .Must(rhythm => string.IsNullOrEmpty(rhythm) ||
                           ValidRhythmAssessments.Contains(rhythm, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid rhythm assessment")
            .When(x => !string.IsNullOrEmpty(x.RhythmAssessment));

        RuleFor(x => x.ECGFindings)
            .MaximumLength(4000).WithMessage("ECG findings cannot exceed 4000 characters");

        RuleFor(x => x.Diagnosis)
            .MaximumLength(2000).WithMessage("Diagnosis cannot exceed 2000 characters");
    }

    private static bool HaveValidBloodPressure(CreateCardioVisitDto dto)
    {
        if (!dto.BloodPressureSystolic.HasValue || !dto.BloodPressureDiastolic.HasValue)
            return true;
        return dto.BloodPressureSystolic.Value > dto.BloodPressureDiastolic.Value;
    }
}

/// <summary>
/// Validator for ECGRecordDto.
/// </summary>
public class ECGRecordValidator : AbstractValidator<ECGRecordDto>
{
    private static readonly string[] ValidRhythms =
    {
        "Sinus Rhythm", "Sinus Bradycardia", "Sinus Tachycardia",
        "Atrial Fibrillation", "Atrial Flutter", "SVT", "VT", "VF",
        "First Degree AV Block", "Second Degree AV Block", "Third Degree AV Block",
        "Bundle Branch Block", "Paced Rhythm"
    };

    public ECGRecordValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(ClinicalVisitValidationMessages.PatientIdRequired);

        RuleFor(x => x.RecordDate)
            .NotEmpty().WithMessage("Record date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Record date cannot be in the future");

        RuleFor(x => x.Rhythm)
            .Must(rhythm => string.IsNullOrEmpty(rhythm) ||
                           ValidRhythms.Contains(rhythm, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid rhythm type")
            .When(x => !string.IsNullOrEmpty(x.Rhythm));

        RuleFor(x => x.HeartRate)
            .InclusiveBetween(20, 300)
            .WithMessage("Heart rate must be between 20 and 300 bpm")
            .When(x => x.HeartRate.HasValue);

        RuleFor(x => x.PRInterval)
            .MaximumLength(50).WithMessage("PR interval cannot exceed 50 characters");

        RuleFor(x => x.QRSDuration)
            .MaximumLength(50).WithMessage("QRS duration cannot exceed 50 characters");

        RuleFor(x => x.QTInterval)
            .MaximumLength(50).WithMessage("QT interval cannot exceed 50 characters");

        RuleFor(x => x.Interpretation)
            .MaximumLength(4000).WithMessage("Interpretation cannot exceed 4000 characters");

        RuleFor(x => x.Abnormalities)
            .MaximumLength(2000).WithMessage("Abnormalities cannot exceed 2000 characters");
    }
}

/// <summary>
/// Validator for EchoResultDto.
/// </summary>
public class EchoResultValidator : AbstractValidator<EchoResultDto>
{
    public EchoResultValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(ClinicalVisitValidationMessages.PatientIdRequired);

        RuleFor(x => x.StudyDate)
            .NotEmpty().WithMessage("Study date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Study date cannot be in the future");

        // Ejection fraction is typically 0-100%
        RuleFor(x => x.EjectionFraction)
            .InclusiveBetween(0, 100)
            .WithMessage("Ejection fraction must be between 0 and 100%")
            .When(x => x.EjectionFraction.HasValue);

        RuleFor(x => x.LeftVentricle)
            .MaximumLength(1000).WithMessage("Left ventricle findings cannot exceed 1000 characters");

        RuleFor(x => x.RightVentricle)
            .MaximumLength(1000).WithMessage("Right ventricle findings cannot exceed 1000 characters");

        RuleFor(x => x.LeftAtrium)
            .MaximumLength(1000).WithMessage("Left atrium findings cannot exceed 1000 characters");

        RuleFor(x => x.RightAtrium)
            .MaximumLength(1000).WithMessage("Right atrium findings cannot exceed 1000 characters");

        RuleFor(x => x.ValveFindings)
            .MaximumLength(2000).WithMessage("Valve findings cannot exceed 2000 characters");

        RuleFor(x => x.WallMotion)
            .MaximumLength(2000).WithMessage("Wall motion findings cannot exceed 2000 characters");

        RuleFor(x => x.Conclusion)
            .MaximumLength(4000).WithMessage("Conclusion cannot exceed 4000 characters");
    }
}

#endregion

#region Ophthalmology Validators

/// <summary>
/// Validator for CreateOphthalmologyVisitDto.
/// </summary>
public class CreateOphthalmologyVisitValidator : CreateClinicalVisitBaseValidator<CreateOphthalmologyVisitDto>
{
    public CreateOphthalmologyVisitValidator()
    {
        RuleFor(x => x.VisualAcuityRight)
            .MaximumLength(50).WithMessage("Visual acuity cannot exceed 50 characters")
            .Must(BeValidVisualAcuity)
            .WithMessage("Invalid visual acuity format (use Snellen notation like 20/20 or decimal like 6/6)")
            .When(x => !string.IsNullOrEmpty(x.VisualAcuityRight));

        RuleFor(x => x.VisualAcuityLeft)
            .MaximumLength(50).WithMessage("Visual acuity cannot exceed 50 characters")
            .Must(BeValidVisualAcuity)
            .WithMessage("Invalid visual acuity format")
            .When(x => !string.IsNullOrEmpty(x.VisualAcuityLeft));

        RuleFor(x => x.IntraocularPressureRight)
            .MaximumLength(20).WithMessage("IOP cannot exceed 20 characters");

        RuleFor(x => x.IntraocularPressureLeft)
            .MaximumLength(20).WithMessage("IOP cannot exceed 20 characters");

        RuleFor(x => x.Diagnosis)
            .MaximumLength(2000).WithMessage("Diagnosis cannot exceed 2000 characters");
    }

    private static bool BeValidVisualAcuity(string? acuity)
    {
        if (string.IsNullOrEmpty(acuity))
            return true;

        // Common formats: 20/20, 6/6, CF (counting fingers), HM (hand motion), LP (light perception), NLP
        var commonValues = new[] { "CF", "HM", "LP", "NLP", "PL", "NPL" };
        if (commonValues.Contains(acuity.ToUpperInvariant()))
            return true;

        // Check Snellen format (number/number) with valid positive values
        if (acuity.Contains('/'))
        {
            var parts = acuity.Split('/');
            if (parts.Length == 2 &&
                decimal.TryParse(parts[0], out var numerator) &&
                decimal.TryParse(parts[1], out var denominator) &&
                numerator > 0 && denominator > 0)
                return true;
        }

        // BUG FIX: Return false for invalid formats instead of allowing anything
        return false;
    }
}

/// <summary>
/// Validator for EyePrescriptionDto.
/// </summary>
public class EyePrescriptionValidator : AbstractValidator<EyePrescriptionDto>
{
    public EyePrescriptionValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(ClinicalVisitValidationMessages.PatientIdRequired);

        RuleFor(x => x.PrescriptionDate)
            .NotEmpty().WithMessage("Prescription date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Prescription date cannot be in the future");

        // Sphere typically -25 to +25 diopters
        RuleFor(x => x.RightSphere)
            .InclusiveBetween(-25m, 25m)
            .WithMessage("Right sphere must be between -25 and +25 diopters")
            .When(x => x.RightSphere.HasValue);

        RuleFor(x => x.LeftSphere)
            .InclusiveBetween(-25m, 25m)
            .WithMessage("Left sphere must be between -25 and +25 diopters")
            .When(x => x.LeftSphere.HasValue);

        // Cylinder typically -10 to +10 diopters
        RuleFor(x => x.RightCylinder)
            .InclusiveBetween(-10m, 10m)
            .WithMessage("Right cylinder must be between -10 and +10 diopters")
            .When(x => x.RightCylinder.HasValue);

        RuleFor(x => x.LeftCylinder)
            .InclusiveBetween(-10m, 10m)
            .WithMessage("Left cylinder must be between -10 and +10 diopters")
            .When(x => x.LeftCylinder.HasValue);

        // Axis 0-180 degrees
        RuleFor(x => x.RightAxis)
            .InclusiveBetween(0, 180)
            .WithMessage("Right axis must be between 0 and 180 degrees")
            .When(x => x.RightAxis.HasValue);

        RuleFor(x => x.LeftAxis)
            .InclusiveBetween(0, 180)
            .WithMessage("Left axis must be between 0 and 180 degrees")
            .When(x => x.LeftAxis.HasValue);

        // Add power typically +0.75 to +3.50
        RuleFor(x => x.RightAdd)
            .InclusiveBetween(0m, 4m)
            .WithMessage("Right add must be between 0 and +4 diopters")
            .When(x => x.RightAdd.HasValue);

        RuleFor(x => x.LeftAdd)
            .InclusiveBetween(0m, 4m)
            .WithMessage("Left add must be between 0 and +4 diopters")
            .When(x => x.LeftAdd.HasValue);

        RuleFor(x => x.PupillaryDistance)
            .MaximumLength(20).WithMessage("Pupillary distance cannot exceed 20 characters");

        RuleFor(x => x.LensType)
            .MaximumLength(100).WithMessage("Lens type cannot exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.PrescriptionDate)
            .WithMessage("Expiry date must be after prescription date")
            .When(x => x.ExpiryDate.HasValue);
    }
}

#endregion

#region Physiotherapy Validators

/// <summary>
/// Validator for CreatePhysioSessionDto.
/// </summary>
public class CreatePhysioSessionValidator : CreateClinicalVisitBaseValidator<CreatePhysioSessionDto>
{
    private static readonly string[] ValidTreatmentTypes =
    {
        "Manual Therapy", "Exercise Therapy", "Electrotherapy", "Hydrotherapy",
        "Heat Therapy", "Cold Therapy", "Ultrasound", "TENS", "Laser Therapy",
        "Traction", "Massage", "Mobilization", "Manipulation"
    };

    public CreatePhysioSessionValidator()
    {
        RuleFor(x => x.AssessmentId)
            .GreaterThan(0).WithMessage("Invalid assessment ID")
            .When(x => x.AssessmentId.HasValue);

        RuleFor(x => x.TreatmentType)
            .Must(type => string.IsNullOrEmpty(type) ||
                         ValidTreatmentTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid treatment type")
            .When(x => !string.IsNullOrEmpty(x.TreatmentType));

        RuleFor(x => x.TechniquesUsed)
            .MaximumLength(2000).WithMessage("Techniques used cannot exceed 2000 characters");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(5, 180)
            .WithMessage("Duration must be between 5 and 180 minutes")
            .When(x => x.DurationMinutes.HasValue);

        // Pain level on 0-10 scale
        RuleFor(x => x.PainLevelBefore)
            .InclusiveBetween(0, 10)
            .WithMessage("Pain level must be between 0 and 10")
            .When(x => x.PainLevelBefore.HasValue);
    }
}

/// <summary>
/// Validator for PhysioAssessmentDto.
/// </summary>
public class PhysioAssessmentValidator : AbstractValidator<PhysioAssessmentDto>
{
    public PhysioAssessmentValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage(ClinicalVisitValidationMessages.PatientIdRequired);

        RuleFor(x => x.AssessmentDate)
            .NotEmpty().WithMessage("Assessment date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Assessment date cannot be in the future");

        RuleFor(x => x.Diagnosis)
            .MaximumLength(2000).WithMessage("Diagnosis cannot exceed 2000 characters");

        RuleFor(x => x.PresentingComplaint)
            .NotEmpty().WithMessage("Presenting complaint is required")
            .MaximumLength(2000).WithMessage("Presenting complaint cannot exceed 2000 characters");

        RuleFor(x => x.HistoryOfPresentIllness)
            .MaximumLength(4000).WithMessage("History cannot exceed 4000 characters");

        RuleFor(x => x.PastMedicalHistory)
            .MaximumLength(4000).WithMessage("Past medical history cannot exceed 4000 characters");

        RuleFor(x => x.ObjectiveFindings)
            .MaximumLength(4000).WithMessage("Objective findings cannot exceed 4000 characters");

        RuleFor(x => x.RangeOfMotion)
            .MaximumLength(2000).WithMessage("Range of motion cannot exceed 2000 characters");

        RuleFor(x => x.StrengthAssessment)
            .MaximumLength(2000).WithMessage("Strength assessment cannot exceed 2000 characters");

        RuleFor(x => x.FunctionalLimitations)
            .MaximumLength(2000).WithMessage("Functional limitations cannot exceed 2000 characters");

        RuleFor(x => x.TreatmentGoals)
            .MaximumLength(2000).WithMessage("Treatment goals cannot exceed 2000 characters");

        RuleFor(x => x.TreatmentPlan)
            .MaximumLength(4000).WithMessage("Treatment plan cannot exceed 4000 characters");

        RuleFor(x => x.PlannedSessions)
            .InclusiveBetween(1, 100)
            .WithMessage("Planned sessions must be between 1 and 100")
            .When(x => x.PlannedSessions.HasValue);
    }
}

#endregion

#region List Request Validator

/// <summary>
/// Validator for ClinicalVisitListRequestDto.
/// </summary>
public class ClinicalVisitListRequestValidator : AbstractValidator<ClinicalVisitListRequestDto>
{
    private static readonly string[] ValidSortFields =
    {
        "VisitDate", "PatientName", "ProviderName"
    };

    public ClinicalVisitListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Invalid patient ID")
            .When(x => x.PatientId.HasValue);

        RuleFor(x => x.ProviderId)
            .GreaterThan(0).WithMessage("Invalid provider ID")
            .When(x => x.ProviderId.HasValue);

        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(x => x.DateTo)
            .WithMessage("Date from must be before or equal to date to")
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);

        RuleFor(x => x.SortBy)
            .Must(field => string.IsNullOrEmpty(field) ||
                          ValidSortFields.Contains(field, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid sort field")
            .When(x => !string.IsNullOrEmpty(x.SortBy));
    }
}

#endregion

#region Extension Methods

/// <summary>
/// Extension methods for registering clinical visit validators.
/// </summary>
public static class ClinicalVisitValidatorExtensions
{
    public static IServiceCollection AddClinicalVisitValidators(this IServiceCollection services)
    {
        // Audiology
        services.AddScoped<IValidator<CreateAudiologyVisitDto>, CreateAudiologyVisitValidator>();
        services.AddScoped<IValidator<UpdateAudiologyVisitDto>, UpdateAudiologyVisitValidator>();
        services.AddScoped<IValidator<AudiogramDto>, AudiogramValidator>();

        // Dental
        services.AddScoped<IValidator<CreateDentalVisitDto>, CreateDentalVisitValidator>();
        services.AddScoped<IValidator<DentalProcedureDto>, DentalProcedureValidator>();
        services.AddScoped<IValidator<ToothRecordDto>, ToothRecordValidator>();

        // Cardiology
        services.AddScoped<IValidator<CreateCardioVisitDto>, CreateCardioVisitValidator>();
        services.AddScoped<IValidator<ECGRecordDto>, ECGRecordValidator>();
        services.AddScoped<IValidator<EchoResultDto>, EchoResultValidator>();

        // Ophthalmology
        services.AddScoped<IValidator<CreateOphthalmologyVisitDto>, CreateOphthalmologyVisitValidator>();
        services.AddScoped<IValidator<EyePrescriptionDto>, EyePrescriptionValidator>();

        // Physiotherapy
        services.AddScoped<IValidator<CreatePhysioSessionDto>, CreatePhysioSessionValidator>();
        services.AddScoped<IValidator<PhysioAssessmentDto>, PhysioAssessmentValidator>();

        // List requests
        services.AddScoped<IValidator<ClinicalVisitListRequestDto>, ClinicalVisitListRequestValidator>();

        return services;
    }
}

#endregion
