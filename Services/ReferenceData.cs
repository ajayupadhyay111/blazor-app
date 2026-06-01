namespace BlazorApp.Services;

/// <summary>
/// Static reference/lookup data for dropdowns. Centralised here so the lists are
/// edited in exactly one place. Course list and Council/CourseType values reflect
/// the client's revised Excel + pen-marked PDF.
/// </summary>
public static class ReferenceData
{
    public static readonly string[] ApplicantTypes = { "Society", "Trust", "Company" };

    public static readonly string[] AppliedForOptions = { "New Course", "Seat Enhancement" };

    // Pen-mark change: was "Nursing Council".
    public static readonly string[] Councils = { "U.P. State Allied & Healthcare Council" };

    // Pen-mark change: was "Degree, Diploma, Master".
    public static readonly string[] CourseTypes = { "Diploma", "Degree (UG)", "Degree (PG)" };

    /// <summary>Courses grouped by Course Type (from client's revised Excel).</summary>
    public static readonly Dictionary<string, string[]> CoursesByType = new()
    {
        ["Diploma"] = new[]
        {
            "Diploma of Anaesthesia and Operation Theatre Technology (D.AOTT)",
            "Diploma of Radiotherapy Technology (D.RT)",
            "Diploma of Dialysis Technology (D.DT)",
            "Diploma of Health Information Management (D.HIM)",
        },
        ["Degree (UG)"] = new[]
        {
            "Bachelor of Medical Laboratory Science (B.MLS)",
            "Bachelor of Emergency Medical Technologist (Paramedic) (B.EMT)",
            "Bachelor of Anaesthesia and Operation Theatre Technology (B.AOTT)",
            "Bachelor of Physiotherapy (B.PT)",
            "Bachelor of Nutrition and Dietetics (Honours) (B.ND)",
            "Bachelor of Optometry (B.OPTOM)",
            "Bachelor of Occupational Therapy (B.OT)",
            "Bachelor of Psychology (B.Psy)",
            "Bachelor of Medical and Psychiatric Social Work (B.MPSW)",
            "Bachelor of Medical Radiology and Imaging Technology (B.MRIT)",
            "Bachelor of Radiation Therapy Technology (B.RTT)",
            "Bachelor of Science in Nuclear Medicine Technology (B.Sc.NMT)",
            "Bachelor of Physician Associates (B.PA)",
            "Bachelor of Dialysis Therapy Technology (B.DTT)",
            "Bachelor of Respiratory Technology (B.RT)",
            "Bachelor of Science in Health Information Management (B.Sc.HIM)",
        },
        ["Degree (PG)"] = new[]
        {
            "Master of Medical Laboratory Science (M.MLS)",
            "Master of Advanced Care Paramedic",
            "Master of Anaesthesia and Operation Theatre Technology (M.AOTT)",
            "Master of Physiotherapy (M.PT)",
            "Master of Nutrition and Dietetics (Honours) (M.ND)",
            "Master of Optometry (M.OPTOM)",
            "Master of Occupational Therapy (M.OT)",
            "Master of Medical Social Work (M.MSW) / Master of Psychiatric Social Work (M.PSW)",
            "Master of Medical Radiology and Imaging Technology (M.MRIT)",
            "Master of Radiation Therapy Technology (M.RTT)",
            "Master of Science in Nuclear Medicine Technology (M.Sc.NMT)",
            "Master of Science in Medical Physics (M.Sc. Medical Physics)",
            "Post Master Diploma in Radiological/Medical Physics OR Advanced Master Degree in Radiological/Medical Physics",
            "Master of Physician Associates (M.PA)",
            "Master of Dialysis Therapy (M.DT)",
            "Master of Respiratory Technology (M.RT)",
            "Master of Science in Health Information Management (M.Sc.HIM)",
        },
    };

    public static string[] CoursesFor(string? courseType) =>
        courseType is not null && CoursesByType.TryGetValue(courseType, out var list) ? list : Array.Empty<string>();

    /// <summary>Districts of Uttar Pradesh (75).</summary>
    public static readonly string[] Districts =
    {
        "Agra","Aligarh","Ambedkar Nagar","Amethi","Amroha","Auraiya","Ayodhya","Azamgarh","Baghpat","Bahraich",
        "Ballia","Balrampur","Banda","Barabanki","Bareilly","Basti","Bhadohi","Bijnor","Budaun","Bulandshahr",
        "Chandauli","Chitrakoot","Deoria","Etah","Etawah","Farrukhabad","Fatehpur","Firozabad","Gautam Buddha Nagar","Ghaziabad",
        "Ghazipur","Gonda","Gorakhpur","Hamirpur","Hapur","Hardoi","Hathras","Jalaun","Jaunpur","Jhansi",
        "Kannauj","Kanpur Dehat","Kanpur Nagar","Kasganj","Kaushambi","Kheri","Kushinagar","Lalitpur","Lucknow","Maharajganj",
        "Mahoba","Mainpuri","Mathura","Mau","Meerut","Mirzapur","Moradabad","Muzaffarnagar","Pilibhit","Pratapgarh",
        "Prayagraj","Raebareli","Rampur","Saharanpur","Sambhal","Sant Kabir Nagar","Shahjahanpur","Shamli","Shravasti","Siddharthnagar",
        "Sitapur","Sonbhadra","Sultanpur","Unnao","Varanasi",
    };
}
