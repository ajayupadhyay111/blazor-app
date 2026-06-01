namespace BlazorApp.Models;

/// <summary>
/// Root model for one affiliation application. This whole object is what gets
/// serialized to localStorage (as JSON) on every autosave, and restored on resume.
/// Keep it a plain POCO so it is trivially (de)serializable and backend-portable later.
/// </summary>
public class AffiliationApplication
{
    public int SchemaVersion { get; set; } = 1;

    /// <summary>draft | submitted</summary>
    public string Status { get; set; } = "draft";

    /// <summary>How far the user has reached in the wizard (1-based step index).</summary>
    public int CurrentStep { get; set; } = 1;

    /// <summary>Generated only at final submission, e.g. UP-AFF-2026-0001.</summary>
    public string? RegistrationCode { get; set; }

    public string? LastSavedUtc { get; set; }
    public string? SubmittedUtc { get; set; }

    public RegistrationInfo Registration { get; set; } = new();
    public ApplicantDetails Applicant { get; set; } = new();
    public List<CourseRow> Courses { get; set; } = new() { new CourseRow() };
    public FormPartI PartI { get; set; } = new();
    public FormPartII PartII { get; set; } = new();
    public Attachments Attachments { get; set; } = new();
    public FeePayment Fee { get; set; } = new();
    public Declaration Declaration { get; set; } = new();
}

public class RegistrationInfo
{
    /// <summary>"0" = No (fresh applicant), "1" = Yes (existing).</summary>
    public string IsRegistered { get; set; } = "0";
}

public class ApplicantDetails
{
    public string ApplicantType { get; set; } = "Society"; // Society | Trust | Company
    public string SocietyName { get; set; } = "";
    public string InstituteName { get; set; } = "";
    public string Address { get; set; } = "";
    public string District { get; set; } = "";
    public string PinCode { get; set; } = "";
    public string ContactPerson { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string Email { get; set; } = "";

    /// <summary>Added per client pen-mark on PDF page 1 (forwarding/inspection officer).</summary>
    public string ForwardingOfficer { get; set; } = "";
}

public class CourseRow
{
    public string AppliedFor { get; set; } = "New Course"; // New Course | Seat Enhancement
    public string Council { get; set; } = "U.P. State Allied & Healthcare Council";
    public string CourseType { get; set; } = ""; // Diploma | Degree (UG) | Degree (PG)
    public string Course { get; set; } = "";
    public int? ExistingSeats { get; set; }
    public int? AppliedSeats { get; set; }
}

public class FormPartI
{
    public int? SeatsAppliedFor { get; set; }
    public string InstitutionType { get; set; } = "PRIVATE";
    public string InstitutionName { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public DateOnly? RegistrationDate { get; set; }
    public string InstitutionAddress { get; set; } = "";
    public string AppliedInstitutionName { get; set; } = "";
    public bool NonProfitableDeclared { get; set; }
}

public class FormPartII
{
    public string ChiefOfficerName { get; set; } = "";
    public string ChiefOfficerDesignation { get; set; } = "";
    public DateOnly? OfficerLetterDate { get; set; }
    public string AffiliatedUniversity { get; set; } = "";
    public string HospitalName { get; set; } = "";
    public string PmjayEmpanelled { get; set; } = "No"; // Yes | No
}

public class Attachments
{
    public UploadedFile? TeachingBlockPhoto { get; set; }   // Attachment-1 (zip)
    public UploadedFile? HostelBlockPhoto { get; set; }     // Attachment-2 (zip)
    public UploadedFile? HospitalPhoto { get; set; }        // Attachment-3 (zip)
    public UploadedFile? StaffDetails { get; set; }         // Attachment-4 (zip) Doctors/Nurses/Allied Healthcare
    public UploadedFile? Affidavit { get; set; }            // Attachment-5 (pdf)
}

public class FeePayment
{
    public const decimal BaseFee = 250000m;
    public const decimal GstRate = 0.18m;
    public static decimal TotalFee => BaseFee + (BaseFee * GstRate); // 2,95,000

    public string Mode { get; set; } = "Online"; // Online | NEFT-RTGS
    public string TransactionNo { get; set; } = "";
    public DateOnly? PaymentDate { get; set; }
    public bool Paid { get; set; }
    public UploadedFile? Proof { get; set; }
}

public class Declaration
{
    public bool Accepted { get; set; }
    public string Place { get; set; } = "";
    public string SignatoryName { get; set; } = "";
    public string? Date { get; set; }
}

/// <summary>A file stored entirely in localStorage as a Base64 data URL (≤ 2 MB).</summary>
public class UploadedFile
{
    public string Name { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; set; }
    public string DataUrl { get; set; } = "";
}
