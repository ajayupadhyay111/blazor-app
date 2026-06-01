# UPSMFAC Affiliation Form — Redesign & Implementation Plan

> **Source form:** https://onlineservices.upsmfac.org/AffiliationTst/frm_Affiliation.aspx
> **Goal:** Purani, buggy government ASP.NET form ko ek modern, clean **Blazor** multi-step wizard mein redesign karna, saara data **browser ke localStorage** mein save karna, aur phir **deploy** karna.
> **Stack jo abhi hai:** .NET 10, Blazor Web App (Interactive Server render mode), Bootstrap 5.
>
> **✅ DECISIONS LOCKED:**
> - **Files:** **Base64 content** localStorage mein, **2 MB/file** limit, preview + remove ke saath.
> - **Hosting:** ~~WASM + Netlify~~ → **revised to Interactive Server + Docker** (reason neeche).
>
> ## ⚠️ Architecture honesty note (zaroor padhein)
> Client ne maanga: *"robust session tracking, concurrent user handling, internet drop par partial
> data securely retained, logging back in."* localStorage se yeh **poori tarah possible nahi**:
> | Requirement | localStorage-only (Phase 1, abhi) | Sachchi zaroorat (Phase 2) |
> |---|---|---|
> | Internet drop → same browser/device pe resume | ✅ Ho gaya | — |
> | Cross-device / "log back in" anywhere | ❌ localStorage ek browser tak seemit | Backend API + DB + login |
> | Concurrent multi-user handling | ❌ client-side concept nahi | Server + DB (per-user rows) |
> | "Securely retained" | ❌ localStorage plaintext | Server-side encrypted storage |
>
> **Phase 1 (deliver kar diya):** functional UI + localStorage autosave/resume — morning demo ke liye.
> Code intentionally clean rakha hai (`DraftStore` ek interface ke peeche async hai) taaki Phase 2
> mein localStorage ki jagah ek **REST API + database (PostgreSQL/SQL Server)** plug ho jaye bina
> UI badle. Yeh Phase 2 estimate ke saath alag se propose kar sakta hoon.

---

## ✅ Build status (Phase 1 complete)
- 8-step wizard banaya gaya, real PDF structure + pen-marked changes ke saath. `dotnet build` = **0 errors**, run = **HTTP 200** `/affiliation`.
- Files: `Models/AffiliationApplication.cs`, `Services/ReferenceData.cs`, `Services/DraftStore.cs`, `wwwroot/js/storage.js`, `Components/Pages/Affiliation.razor`, `Components/Wizard/*` (Stepper, FileUpload, Step1–7, ReviewSummary), dark-blue theme `wwwroot/app.css`.
- Deployment: `Dockerfile` + `DEPLOYMENT.md` (Render/Railway/Azure + custom subdomain steps).

---

## 0. Yeh document kaise padhein

- **Section 1–2:** Original form ka analysis (kya scrape hua, kya nahi, aur kyun).
- **Section 3:** Poori field list — har step ke saare input fields (yahi "all possible input fields" hai).
- **Section 4:** Naya UI/UX design (kaisa dikhega).
- **Section 5:** **Business logic — "kahan click karne se kya hoga"** (yeh aapne specially manga tha).
- **Section 6:** localStorage data model (data kaise save hoga).
- **Section 7:** Code structure (kaunsi file kya karegi).
- **Section 8:** Step-by-step build plan (phases).
- **Section 9:** Deployment process (2 options).

---

## 1. Original form ka analysis (scraping result)

Original page ek **ASP.NET WebForms** page hai. Pehli screen sirf ek **gate** hai:

| Field | Type | Detail |
|---|---|---|
| "Are you Registered on this portal (For Fresh Applicants)?" | Radio | Options: **No** / **Yes** |

- **Yes** → already submitted applicant, registration code + mobile se login.
- **No** → naya (fresh) applicant — yahi se asli lamba form khulta hai (postback ke baad).

Asli form ke saare sections page ke HTML mein `CollapsiblePanelExtender` ke roop mein milte hain. Inke control naam se poora form flow confirm hota hai (yeh **actual government structure hai**, guess nahi):

```
1.  Authentication                    → Mobile/OTP se registration
2.  Applicant_Details                 → Institute + Trust/Society + applicant info
3.  Application_Course_Details        → New College ya Seat Enhancement + course selection
4.  Prospectus_Fee_Details            → Prospectus fee dikhana
5.  ProspectusFeePayment_Details      → Prospectus fee payment
6.  ProspectusDownload_Details        → Prospectus download
7.  Form_Details_1                    → Detailed application — Part 1 (land/building/location)
8.  Form_Details_2                    → Detailed application — Part 2 (faculty/lab/library/hospital)
9.  Relative_Photographs_Details      → Photo uploads
10. Relative_Documents_Details        → Document uploads
11. ApplicationFee_Details            → Application fee dikhana
12. ApplicationFeePayment_Details     → Application fee payment (online)
13. ApplicationFee_NEFT_RTGS_Details  → Alternative NEFT/RTGS payment
14. Enclosures_Details                → Final checklist + Submit
```

### Scraping ki honest limitation
Inner fields ke **exact label text** server se nahi nikal paaye, kyunki:
- Form har step postback se load karta hai, aur
- Server ek **web-farm/cluster** hai jisme `viewstate MAC` cross-node fail ho jaata hai (`Validation of viewstate MAC failed`). Yahi original site ki buggy-ness ka ek bada karan hai.

Isliye neeche di gayi field list **panel structure (confirmed) + UPSMFAC affiliation domain knowledge** se banayi gayi hai. Yeh practically complete hai; build karte waqt agar koi field add/remove karni ho to woh trivial hai (config-driven design — Section 7 dekho).

---

## 2. Redesign ka core idea

| Purana (original) | Naya (hamara) |
|---|---|
| ASP.NET postback, har click pe full page reload | Blazor — instant, no reload |
| 14 collapsible panels ek hi lambi page pe | Clean **step-by-step wizard** (progress bar ke saath) |
| Server pe data, login zaroori | Sab kuch **localStorage** mein, auto-save |
| Toota viewstate, errors | Client-side validation, koi server dependency nahi |
| Purana 2010-era UI | Modern Bootstrap 5 cards, responsive, mobile-friendly |

---

## 3. Poori field list (ACTUAL form, client ke PDF + pen-marked changes ke hisaab se)

> ✅ Yeh list ab client dwara diye gaye **6-page PDF (real form)** + **Excel course list** + **pen-marked corrections** par based hai — pehle ka domain-guess version replace kar diya gaya.
> Har field ke saamne: **type**, **(R)** = required. 🖊️ = client ne pen se change/add manga.
> Wizard steps original form ke 6 sections ko follow karenge.

### Step 1 — Registration gate
| Field | Type | Notes |
|---|---|---|
| Are you Registered on this portal (For Fresh Applicants)? | Radio (R) | **No** (naya applicant → form khulega) / **Yes** (existing → reg. code + mobile se resume) |

### Step 2 — Applicant's Details
| Field | Type | Notes |
|---|---|---|
| Applicant Type | Dropdown (R) | **Society / Trust / Company** |
| Name of Society/Trust/Company | Text (R) | |
| Proposed Institute Name | Text (R) | |
| Address | Textarea (R) | |
| District | Dropdown (R) | UP ke districts |
| Pin Code | Text (R) | 6 digit |
| Contact Person Name | Text (R) | |
| Phone | Text | landline/STD |
| Mobile | Text (R) | 10 digit, `^[6-9]\d{9}$` |
| Email | Text (R) | email validation |
| 🖊️ **Forwarding / Inspection Officer Name** (अग्रेषण अधिकारी — approving authority jo proposal forward karega) | Text | **PDF page-1 pen-note se ADD** — confirm karein (label/required?) |
| → Button: **Save & Proceed Next** | | data save → Step 3 |

### Step 3 — Application for the Course Details
| Field | Type | Notes |
|---|---|---|
| Applied for | Dropdown (R) | **New Course** / **Seat Enhancement** |
| 🖊️ Council | Dropdown (R) | ~~Nursing Council~~ → **U.P. State Allied & Healthcare Council** *(pen change)* |
| 🖊️ Course Type | Dropdown (R) | ~~Degree, Diploma, Master~~ → **Diploma / Degree (UG) / Degree (PG)** *(pen change)* |
| 🖊️ Select Course | Dropdown (R) | Excel ki nayi list, **Course Type ke hisaab se filter** (Appendix A dekho) |
| Existing Seats | Number | sirf **Seat Enhancement** ke liye dikhe |
| Applied Seats | Number (R) | |
| → Button: **Save & Proceed Next** | | |

> Original form ek time pe ek course add karta hai ("Applied Course Details updated successfully"). Hamare redesign mein **dynamic table** rakhenge — "Add Course" se multiple courses, har row: Council, Course Type, Course, Existing/Applied seats. Delete per row.

### Step 4 — Form Details Part-I (About Society/Trust/Company & Training Center)
> Header: *"Please fill the form below in English language only."* (Hindi font note original ka — hamare app mein zaroorat nahi.)

| Field | Type | Notes |
|---|---|---|
| No. of Seats Applied for | Number (R) | |
| Type of institution | Dropdown/Text (R) | e.g. **PRIVATE** |
| Name of institution (Society/Trust/Company) | Text (R) | auto-fill Step 2 se |
| Institution Registration Number | Text (R) | |
| Institution Registration Date (valid honi chahiye) | Date (R) | dd/MM/yyyy |
| Institution Address | Textarea (R) | |
| Name of Applied Institution | Text (R) | |
| Declaration: Non-Profitable, gramin/25km dayre ke andar, registered + transferred licence | Checkbox/Note | original form ki condition |
| **Document uploads (Part-I)** — Registration certificate, etc. | File (pdf/zip) | multiple "Choose File" — Appendix B |

### Step 5 — Form Details Part-II (Chief Officer / University / Hospital)
> Hindi-labelled section — hum English + Hindi dono label denge.

| Field | Type | Notes |
|---|---|---|
| 🖊️ Chief / Nodal Officer details (मुख्य अधिकारी) | Text group | **"Allied Healthcare"** terminology (pen note: Nursing/Paramedic hatao) |
| Officer name / designation / signature date | Text + Date | dd/MM/yyyy |
| Affiliated University ka naam (संबद्ध विश्वविद्यालय) | Text | |
| Letter of Intent / University letter | File (pdf) | |
| ANC Register photocopy | File (zip/pdf) | |
| Is the Hospital Empanelled with PMJAY? | Radio (R) | **Yes / No** |
| Hospital ka naam / details | Text | |
| 🖊️ Mukhya Vidhayi/Niyamak Adhikari (regulatory authority) provided patra | File + Date | **Allied & Healthcare** terminology |
| → Button: **Save Form Details Part-II & Proceed Next** | | |

### Step 6 — Upload Related Photographs / Documents
| Attachment | Field | Type | Notes |
|---|---|---|---|
| Attachment-1 | Teaching Block Photograph (Front, Back & Side View) | File (R) | **zip** |
| Attachment-2 | Hostel Block Photograph (Front, Back & Side View) | File | **zip** |
| Attachment-3 | Hospital Photograph (Front, Back & Side View) | File | **zip** |
| Attachment-4 🖊️ | Hospital Staff Details (Doctors/Nurses/**Allied Healthcare**) | File (R) | **zip** — ~~Paramedics~~ → **Allied Healthcare** *(pen change)* |
| Attachment-5 | Affidavit on stamp paper attested by a Notary | File (R) | **pdf** |
| → Button: **Upload File(s) & Proceed Next** | | | |

### Step 7 — Application Fee & Inspection Fee Payment
| Field | Type | Notes |
|---|---|---|
| Fee (read-only) | Display | **Rs. 2,50,000/- + 18% GST** = ₹2,95,000 (original PDF se) |
| Payment Mode | Radio (R) | Online (Payment Gateway) / NEFT-RTGS |
| **Pay Now** button | Button | mock payment (gateway nahi) → dummy success + txn id |
| UTR / Transaction No. | Text (R) | |
| Payment Date | Date (R) | |
| Payment Proof | File | optional |

### Step 8 — Print Preview & Final Submission
| Element | Type | Notes |
|---|---|---|
| Read-only summary (saara data ek jagah) | View | sab steps ka preview |
| Declaration / self-attestation | Checkbox (R) | |
| **Print Preview** button | Button | browser print (PDF) |
| **Final Submission** button | Button | validate → Registration Code generate → localStorage `submitted` |

> **Files localStorage mein:** Base64, **2 MB/file limit** (locked decision). Zip/pdf bhi Base64 string ki tarah. 2 MB se bada → error.

---

### Appendix A — Course dropdown data (Excel se, Course Type ke hisaab se grouped)

> Step 3 ka **"Select Course"** dropdown, **Course Type** select karne par filter hoga. `Services/ReferenceData.cs` mein yeh list jayegi.

**Course Type = Diploma**
- Diploma of Anaesthesia and Operation Theatre Technology (D.AOTT)
- Diploma of Radiotherapy Technology (D.RT)
- Diploma of Dialysis Technology (D.DT)
- Diploma of Health Information Management (D.HIM)

**Course Type = Degree (UG)**
- Bachelor of Medical Laboratory Science (B.MLS)
- Bachelor of Emergency Medical Technologist (Paramedic) (B.EMT)
- Bachelor of Anaesthesia and Operation Theatre Technology (B.AOTT)
- Bachelor of Physiotherapy (B.PT)
- Bachelor of Nutrition and Dietetics (Honours) (B.ND)
- Bachelor of Optometry (B.OPTOM)
- Bachelor of Occupational Therapy (B.OT)
- Bachelor of Psychology (B.Psy)
- Bachelor of Medical and Psychiatric Social Work (B.MPSW)
- Bachelor of Medical Radiology and Imaging Technology (B.MRIT)
- Bachelor of Radiation Therapy Technology (B.RTT)
- Bachelor of Science in Nuclear Medicine Technology (B.Sc.NMT)
- Bachelor of Physician Associates (B.PA)
- Bachelor of Dialysis Therapy Technology (B.DTT)
- Bachelor of Respiratory Technology (B.RT)
- Bachelor of Science in Health Information Management (B.Sc.HIM)

**Course Type = Degree (PG)**
- Master of Medical Laboratory Science (M.MLS)
- Master of Advanced Care Paramedic
- Master of Anaesthesia and Operation Theatre Technology (M.AOTT)
- Master of Physiotherapy (M.PT)
- Master of Nutrition and Dietetics (Honours) (M.ND)
- Master of Optometry (M.OPTOM)
- Master of Occupational Therapy (M.OT)
- Master of Medical Social Work (M.MSW) / Master of Psychiatric Social Work (M.PSW)
- Master of Medical Radiology and Imaging Technology (M.MRIT)
- Master of Radiation Therapy Technology (M.RTT)
- Master of Science in Nuclear Medicine Technology (M.Sc.NMT)
- Master of Science in Medical Physics (M.Sc. Medical Physics)
- Post Master Diploma in Radiological/Medical Physics OR Advanced Master Degree in Radiological/Medical Physics
- Master of Physician Associates (M.PA)
- Master of Dialysis Therapy (M.DT)
- Master of Respiratory Technology (M.RT)
- Master of Science in Health Information Management (M.Sc.HIM)

---

### Appendix B — Client ke pen-marked changes (PDF se), ek jagah summary

| # | Page | Original | Naya (client change) | Status |
|---|---|---|---|---|
| 1 | p2 | Council: "Nursing Council" | **U.P. State Allied & Healthcare Council** | ✅ apply |
| 2 | p2 | Course Type: "Degree, Diploma, Master" | **Diploma / Degree (UG) / Degree (PG)** | ✅ apply |
| 3 | p2 | Select Course (purani list) | **Excel ki nayi list** (Appendix A) | ✅ apply |
| 4 | p4/p5 | Attachment-4: "...Paramedics" | **"...Allied Healthcare"** | ✅ apply |
| 5 | p3-p6 | "Paramedic / Nursing" terminology jagah-jagah | **"Allied & Healthcare"** | ✅ apply |
| 6 | p1 | (naya) | **Forwarding/Inspection Officer (अग्रेषण अधिकारी) name** field add | ⚠️ confirm label & required |
| 7 | p3 | kuch **date** fields pe "2000.." strike | naya default/format | ⚠️ exact value confirm |

> ⚠️ wale 2 points (6 & 7) ki handwriting poori clear nahi thi — Section 10 mein confirm ke liye rakha hai. Baaki sab apply ho jayenge.

---

## 4. Naya UI / UX design

```
┌─────────────────────────────────────────────────────────┐
│  UPSMFAC  ·  Affiliation Application                      │  ← Header
├─────────────────────────────────────────────────────────┤
│ ① Reg ─ ② Inst ─ ③ Course ─ ④ Fee ─ ⑤ Land ─ ... ─ ⑩ Submit│  ← Stepper / progress bar
├─────────────────────────────────────────────────────────┤
│                                                           │
│   [ Current step ke fields — Bootstrap card mein ]        │
│                                                           │
│   ┌─ Field group ───────────────────────────────┐        │
│   │  Label *                                      │        │
│   │  [ input ]                                    │        │
│   │  ⚠ validation message (agar error)            │        │
│   └───────────────────────────────────────────────┘      │
│                                                           │
├─────────────────────────────────────────────────────────┤
│  [← Back]        Auto-saved ✓ 2:34 PM        [Next →]     │  ← Footer nav
└─────────────────────────────────────────────────────────┘
```

**Design principles:**
- Ek time pe **ek step** (cognitive load kam).
- Upar **clickable stepper** — completed steps green, current blue, pending grey.
- Har field pe inline validation (red border + message).
- **Auto-save indicator** — har change pe localStorage mein save, footer mein "Saved ✓" dikhe.
- Mobile responsive (Bootstrap grid).
- Step 10 pe ek **read-only summary/preview** (saara bhara hua data ek jagah) + **Print/PDF** button.

---

## 5. Business logic — "Kahan click karne se kya hoga"

> Yeh section har interactive element ka exact behaviour describe karta hai.

### Landing / Step 1
- **Page load** → localStorage check. Agar pehle se draft hai → "Resume previous application?" banner (Resume / Start New).
- **"New Registration" radio** select → mobile + email + "Send OTP" button dikhe.
- **"Send OTP" button click** → mobile validate (10 digit). Valid → OTP box enable, dummy OTP generate aur (demo ke liye) screen pe toast `OTP: 123456`. Button "Resend OTP (30s)" countdown start.
- **"Verify OTP" button click** → entered OTP == generated? → step 1 complete (green tick), **Next** enable. Galat → red error "Invalid OTP".
- **"Next" button** → sirf tab enable jab current step ke **required fields valid** ho. Click → data localStorage mein save, step 2 pe jao, stepper update.

### Step 2 (Institute Details)
- **District dropdown** change → (future) tehsil filter; abhi sirf value save.
- **Pincode** blur → 6-digit validate; galat → error message.
- **Next** → validate sab required → save → Step 3.
- **Back** → save → Step 1 (data preserve).

### Step 3 (Courses) — dynamic table
- **"Application Type" radio**:
  - **New College Opening** select → "Existing Seats" column hide (kyunki naya college hai).
  - **Seat Enhancement** select → "Existing Seats" column **show** (R).
- **"Add Course" button click** → table mein nayi blank row add.
- **Row "🗑 Delete" click** → woh row remove (kam se kam 1 row honi chahiye).
- Course/seats change → Step 4 ka fee **auto-recalculate**.
- **Next** → kam se kam 1 course required.

### Step 4 & 9 (Fees / Payment)
- **Fee amount** read-only — Step 3 ke courses se calculate (rule: e.g. ₹X per course + base). Logic ek `FeeCalculator` service mein.
- **Payment Mode = "Online"** → ek mock **"Pay Now"** button (real gateway nahi; click pe dummy success + auto-fill transaction id + date).
- **Payment Mode = "NEFT/RTGS"** → UTR, Bank, Date fields **show** (R), + payment proof upload.
- **"Pay Now" / proof attach** → step complete tick.

### Step 5 & 6 (Detailed form) — conditional fields
- **Land Ownership = "Leased"** → "Lease Period" field show (R).
- **Building Status = "Under construction"** → warning note dikhe ("completion certificate baad mein").
- **Attached Hospital = "None"** → bed count hide; **"Own"/"MoU"** → bed count show (R).
- **Hostel = "Yes"** → capacity show.
- **"Add Faculty" button** → faculty table mein row add (Name, Designation, Qualification, Experience). Delete per row.

### Step 7 & 8 (Uploads)
- **File select** → instant validation:
  - Allowed type? (image: jpg/png; doc: pdf)
  - Size ≤ limit (e.g. 2 MB)? Bada → error "File too large for local storage".
  - Valid → file ko Base64 mein convert, localStorage mein save, **thumbnail/preview + filename + "Remove" button** dikhe.
- **"Remove" click** → file localStorage se hata do, slot wapas empty.

### Step 10 (Submit)
- **Page load** → saare steps ka **read-only summary** render (sab data ek jagah).
- **Koi required cheez missing** → us step ka link red, "⚠ Step X incomplete — click to fix".
- **Declaration checkbox** + sab enclosure checkboxes ticked hone pe hi **"Submit Application"** enable.
- **"Submit" click** →
  1. Final validation (sab steps).
  2. Registration Code generate (`UP-AFF-2026-XXXX`).
  3. Pura object localStorage mein `submitted` status ke saath save (`submittedApplications` array mein push).
  4. Draft clear / archive.
  5. **Success screen** — Registration Code + "Download as PDF/Print" + "View / New Application".
- **"Print / Download PDF" click** → browser print dialog (CSS `@media print` se clean layout) — applicant apni copy rakh sake.

### Global behaviours
- **Har field change** → debounce 500ms → localStorage auto-save → footer "Saved ✓ HH:MM".
- **Stepper step click** → agar woh step ya usse pehle ke complete → jump allowed; aage ke locked (jab tak current valid na ho).
- **Browser refresh / band karke wapas** → draft localStorage se reload, jahan chhoda tha wahin.

---

## 6. localStorage data model

**Key:** `upsmfac_affiliation_draft` (current draft), `upsmfac_affiliation_submitted` (submitted list).

```jsonc
// upsmfac_affiliation_draft  (single object)
{
  "schemaVersion": 1,
  "status": "draft",                // draft | submitted
  "lastSavedUtc": "2026-06-01T...",
  "currentStep": 3,
  "registration": { "mobile": "", "email": "", "otpVerified": true },
  "institute":    { "name": "", "trustName": "", "district": "", ... },
  "courses":      [ { "course": "GNM", "existingSeats": 0, "appliedSeats": 60 } ],
  "prospectusFee":{ "amount": 0, "mode": "Online", "utr": "", "date": "" },
  "landBuilding": { "totalLand": 0, "ownership": "Owned", ... },
  "facilities":   { "faculty": [ {...} ], "labs": 0, "library": 0, ... },
  "photos":       { "buildingFront": { "name":"a.jpg","type":"image/jpeg","dataUrl":"data:image/jpeg;base64,..." } },
  "documents":    { "trustCert": { "name":"x.pdf","type":"application/pdf","dataUrl":"..." } },
  "applicationFee":{ "amount": 0, "mode":"NEFT", "utr":"", "amountPaid":0 },
  "enclosures":   { "checklist": ["trustCert","land",...], "declaration": true, "place":"", "signatory":"" }
}
```

```jsonc
// upsmfac_affiliation_submitted  (array — har submit push)
[ { "registrationCode": "UP-AFF-2026-0001", "submittedUtc": "...", "data": { ...above... } } ]
```

**File storage rule:** files Base64 `dataUrl` ke roop mein. localStorage ka limit ~5 MB hota hai, isliye:
- Per-file limit **2 MB** enforce.
- Total draft size monitor; ~4 MB cross hone pe warning ("Storage almost full — large files cannot be saved locally").
- Library: **Blazored.LocalStorage** (clean async API) — `await LocalStorage.SetItemAsync(key, obj)` JSON serialize khud karega.

---

## 7. Code structure (kaunsi file kya karegi)

```
BlazorApp/
├─ Models/
│  ├─ AffiliationApplication.cs     // poora data model (upar wala JSON ka C# class)
│  ├─ CourseRow.cs, FacultyRow.cs   // dynamic table rows
│  └─ UploadedFile.cs               // name/type/dataUrl
├─ Services/
│  ├─ IDraftStore.cs / DraftStore.cs    // localStorage read/write/auto-save (Blazored wrap)
│  ├─ FeeCalculator.cs                  // fee rules ek jagah
│  └─ ReferenceData.cs                  // UP districts, courses, sessions lists
├─ Components/
│  ├─ Pages/
│  │  └─ Affiliation.razor          // /affiliation — wizard host (current step render, nav)
│  ├─ Wizard/
│  │  ├─ Stepper.razor              // upar wala progress stepper
│  │  ├─ Step1Registration.razor
│  │  ├─ Step2Institute.razor
│  │  ├─ Step3Courses.razor         // dynamic course table
│  │  ├─ Step4ProspectusFee.razor
│  │  ├─ Step5LandBuilding.razor
│  │  ├─ Step6Facilities.razor      // dynamic faculty table
│  │  ├─ Step7Photographs.razor     // file upload + preview
│  │  ├─ Step8Documents.razor
│  │  ├─ Step9ApplicationFee.razor
│  │  └─ Step10Review.razor         // summary + submit + print
│  └─ Shared/
│     ├─ FileUpload.razor           // reusable upload+Base64+preview+remove
│     └─ FieldGroup.razor           // label + input + validation reusable
```

**Validation:** `System.ComponentModel.DataAnnotations` + Blazor `EditForm`/`DataAnnotationsValidator` har step model pe. Custom rules (mobile regex, pincode) attributes se.

**Program.cs changes:** `builder.Services.AddBlazoredLocalStorage();` + scoped `DraftStore`, `FeeCalculator` register.

**csproj:** `<PackageReference Include="Blazored.LocalStorage" Version="4.*" />`.

> **Note (localStorage + Interactive Server):** localStorage JS interop sirf circuit connect hone ke baad (after first render) chalega. Isliye draft load `OnAfterRenderAsync(firstRender)` mein karenge, server-prerender ke time nahi. Yeh handle karna zaroori hai warna `JSException` aayega.

---

## 8. Build plan (phases)

1. **Phase 0 — Setup:** Blazored.LocalStorage add, NavMenu mein "Affiliation Form" link, route `/affiliation`, demo pages (Counter/Weather) hata do.
2. **Phase 1 — Skeleton:** `AffiliationApplication` model, `DraftStore`, `Affiliation.razor` host + `Stepper` + Back/Next nav (dummy steps).
3. **Phase 2 — Steps 1–3:** Registration (mock OTP), Institute, Courses (dynamic table) + validation + auto-save.
4. **Phase 3 — Steps 4–6:** Fee/payment (mock), Land/Building, Facilities (faculty table) + conditional fields.
5. **Phase 4 — Steps 7–8:** Reusable `FileUpload` (Base64, preview, size guard) + photo/document steps.
6. **Phase 5 — Steps 9–10:** App fee, Review summary, submit → registration code, success screen, print CSS.
7. **Phase 6 — Polish:** responsive check, empty/refresh/resume flows, storage-full warning, final styling.
8. **Phase 7 — Deploy** (Section 9).

---

## 9. Deployment process

App **Blazor Web App (Interactive Server)** hai — isliye ek **server host** chahiye (static-only host kaam nahi karega jab tak hum WASM mein convert na karein — niche dono options diye hain).

### Option A — Server host rakhein (current architecture, recommended easy path)
Best free/cheap options: **Azure App Service** ya **Render/Railway** (Docker).

**Steps (Azure App Service):**
1. Local test: `dotnet run` → `https://localhost:xxxx/affiliation` pe verify.
2. Publish: `dotnet publish -c Release -o ./publish`
3. Azure portal → "Create a resource" → **Web App** → Runtime stack **.NET 10**, OS Linux/Windows.
4. VS Code "Azure App Service" extension ya CLI:
   ```bash
   az webapp up --name upsmfac-affiliation --runtime "DOTNET|10.0" --sku F1
   ```
5. Browser → `https://upsmfac-affiliation.azurewebsites.net/affiliation`.

**Docker (Render/Railway/any VPS) ke liye `Dockerfile`:**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","BlazorApp.dll"]
```
Render: New → Web Service → repo connect → auto-detect Dockerfile → deploy. Done.

### Option B — Pure static (GitHub Pages / Netlify, FREE, no server)
Kyunki saara data localStorage mein hai, **server ki zaroorat hi nahi**. Iske liye project ko **Blazor WebAssembly (Standalone)** mein convert karna padega:
1. Naya `Microsoft.NET.Sdk.BlazorWebAssembly` project, components copy.
2. Render mode hatado (WASM by default client-side; JS interop/localStorage direct chalega — Option A wali prerender problem bhi khatam).
3. `dotnet publish -c Release` → output `wwwroot/`.
4. **GitHub Pages:** `wwwroot` ko `gh-pages` branch pe push; `.nojekyll` file add; `index.html` ka `<base href>` repo-name pe set.
5. **Netlify (easier):** repo connect → build command `dotnet publish -c Release -o out` → publish dir `out/wwwroot` → deploy. Free HTTPS URL milega.

> **Recommendation:** Demo/portfolio ke liye **Option B (WASM + Netlify)** — bilkul free, koi server cost nahi, aur localStorage ke saath perfectly fit. Agar future mein server-side DB/admin chahiye to **Option A**.

---

## 10. Open questions (build shuru karne se pehle confirm karein)

1. **Hosting** — Option A (server) ya Option B (free static WASM)? Isse architecture decide hoga.
2. **Courses ki exact list** — main GNM/ANM/B.Sc Nursing/DMLT/Pharmacy jaisi standard list daal raha hoon. Aapke paas official course list ho to bata dena.
3. **Fee rules** — fee calculation ka exact formula (per course rate, base amount)? Filhaal placeholder rule rakhunga.
4. **Language** — UI English mein, ya Hindi/bilingual labels chahiye?
5. **Files** — 2 MB/file localStorage limit theek hai, ya sirf metadata store karein (file content nahi)?

---

*Yeh plan padh lo. Confirm/changes batao, phir main Phase 0 se code likhna shuru kar dunga.*
