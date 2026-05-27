using System.Diagnostics;
using System.Text.Json;
using ART122.Data;
using Microsoft.AspNetCore.Hosting;

namespace ART122.Services;

// ─── DTO ─────────────────────────────────────────────────────────────────────

public class Art122FormDto
{
    // ── Identity ─────────────────────────────────────────────────────────────
    public string FullName { get; set; } = string.Empty;
    public int BP { get; set; }
    public string Etablissement { get; set; } = string.Empty;
    public string Activite { get; set; } = string.Empty;
    public string NIF { get; set; } = string.Empty;
    public string RC { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // ── Tax office ────────────────────────────────────────────────────────────
    public string QabadhaDaraa { get; set; } = string.Empty;
    public string DateEtatFiscal { get; set; } = string.Empty;

    // ── Declaration stamp ─────────────────────────────────────────────────────
    public string DeclarationNumber { get; set; } = string.Empty;
    public string DeclarationCity { get; set; } = string.Empty;

    // ── Amounts (DZD) ─────────────────────────────────────────────────────────
    public decimal Droit { get; set; }
    public decimal PA { get; set; }
    public decimal PR { get; set; }

    // ── Signature ─────────────────────────────────────────────────────────────
    public string SignatureCity { get; set; } = string.Empty;
    public string SignatureDate { get; set; } = string.Empty;

    // ── Payment method ────────────────────────────────────────────────────────
    public bool DafaaWahida { get; set; } = true;
    public bool AlaAqsat { get; set; }
}

// ─── INTERFACE ────────────────────────────────────────────────────────────────

public interface IArt122PdfService
{
    Task<byte[]> FillFormAsync(Art122FormDto dto);
    Task<byte[]> FillFromRedevableAsync(int redevableId);
    Task<byte[]> FillFromDeclarationAsync(Declaration declaration);
}

// ─── SERVICE ─────────────────────────────────────────────────────────────────

public class Art122PdfService : IArt122PdfService
{
    private readonly IRedevableService _redevableService;
    private readonly IAppSettingsService _settingsService;
    private readonly string _templatePath;
    private readonly string _scriptPath;

    // ── Static / non-org hardcoded values ────────────────────────────────────
    private const string HardcodedPeriodeDette = " 2025 - 2012";

    // ── PDF field IDs ─────────────────────────────────────────────────────────
    private static class F
    {
        public const string StampNumber = "fill_5";
        public const string StampCity = "fill_6";
        public const string Direction = "Text10";
        public const string Service = "Text11";
        public const string DirectionFr = "Text12";
        public const string ServiceFr = "Text13";
        public const string FullName = "Text14";
        public const string BP = "Text18";
        public const string Etablissement = "Text15";
        public const string Activite = "Text16";
        public const string NIF = "Text17";
        public const string RC = "Text66";
        public const string Adresse = "Text19";
        public const string Telephone = "Text20";
        public const string Email = "Text21";
        public const string QabadhaDaraa = "Text22";
        public const string _unused23 = "Text23";
        public const string DateEtatFiscal = "Text24";
        public const string PeriodeDette = "Text25";
        public const string Droit = "Text26";
        public const string PA = "Text27";
        public const string PR = "Text28";
        public const string MontantTotal = "Text29";
        public const string TotalWords = "Text30";
        public const string MontantRestant = "Text31";
        public const string MontantRestantWords = "Text32";
        public const string CheckDafaaWahida = "Check Box33";
        public const string CheckAlaAqsat = "Check Box34";
        public const string SignatureCity = "Text35";
        public const string SignatureDate = "Text36";
    }

    public Art122PdfService(
        IRedevableService redevableService,
        IAppSettingsService settingsService,
        IWebHostEnvironment env)
    {
        _redevableService = redevableService;
        _settingsService = settingsService;
        _templatePath = Path.Combine(env.WebRootPath, "templates", "art122_template.pdf");
        _scriptPath = Path.Combine(env.WebRootPath, "scripts", "fill_art122.py");
    }

    // ── Build DTO from redevable ──────────────────────────────────────────────

    public async Task<byte[]> FillFromRedevableAsync(int redevableId)
    {
        var org = await _settingsService.GetAsync();

        var summaries = await _redevableService.GetTaxSummaryAsync();
        var summary = summaries.FirstOrDefault(r => r.RedevableId == redevableId)
            ?? throw new InvalidOperationException($"Redevable {redevableId} not found.");

        var allRedevables = await _redevableService.GetAllAsync();
        var redevable = allRedevables.FirstOrDefault(r => r.Id == redevableId)
            ?? throw new InvalidOperationException($"Redevable {redevableId} not found.");

        var dto = new Art122FormDto
        {
            FullName = redevable.FullName,
            Etablissement = redevable.Etablissement,
            Activite = redevable.Activite,
            NIF = redevable.NIF.ToString(),
            Adresse = redevable.Adresse,
            Telephone = redevable.Telephone,
            Email = redevable.Email,
            BP = summary.BP,

            Droit = summary.TotalDroit,
            PA = summary.TotalPA,
            PR = summary.TotalPR,

            QabadhaDaraa = org.Qabada,                                          // ← من الإعدادات
            DateEtatFiscal = redevable.DateExtraitDeRole.ToString("yyyy/MM/dd"),
            DeclarationNumber = "1",
            DeclarationCity = redevable.DateDeclaration.ToString("yyyy/MM/dd"),

            SignatureCity = org.Qabada,                                            // ← من الإعدادات
            SignatureDate = redevable.DateDeclaration.ToString("yyyy/MM/dd"),
            DafaaWahida = true,
        };

        return await FillFormAsync(dto);
    }

    // ── Build DTO from declaration ────────────────────────────────────────────

    public async Task<byte[]> FillFromDeclarationAsync(Declaration declaration)
    {
        var org = await _settingsService.GetAsync();

        var allRedevables = await _redevableService.GetAllAsync();
        var redevable = allRedevables.FirstOrDefault(r => r.Id == declaration.RedevableInfoId)
            ?? throw new InvalidOperationException(
                   $"Redevable {declaration.RedevableInfoId} not found.");

        var dto = new Art122FormDto
        {
            // ── Identity ─────────────────────────────────────────────────────
            FullName = redevable.FullName,
            Etablissement = redevable.Etablissement,
            Activite = redevable.Activite,
            NIF = redevable.NIF.ToString(),
            BP = redevable.BP,
            Adresse = redevable.Adresse,
            Telephone = redevable.Telephone,
            Email = redevable.Email,

            // ── Amounts ───────────────────────────────────────────────────────
            Droit = declaration.Droit,
            PA = declaration.PA,
            PR = declaration.PR,

            // ── Declaration stamp ─────────────────────────────────────────────
            DeclarationNumber = declaration.Number.ToString(),
            DeclarationCity = declaration.Date.ToString("yyyy/MM/dd"),

            // ── Tax office ────────────────────────────────────────────────────
            QabadhaDaraa = org.Qabada,                                           // ← من الإعدادات
            DateEtatFiscal = redevable.DateExtraitDeRole.ToString("yyyy/MM/dd"),

            // ── Payment method ────────────────────────────────────────────────
            DafaaWahida = declaration.DafaaWahida,
            AlaAqsat = declaration.AlaAqsat,

            // ── Signature ─────────────────────────────────────────────────────
            SignatureCity = org.Qabada,                                            // ← من الإعدادات
            SignatureDate = declaration.Date.ToString("yyyy/MM/dd"),
        };

        return await FillFormAsync(dto);
    }

    // ── Core filler ───────────────────────────────────────────────────────────

    public async Task<byte[]> FillFormAsync(Art122FormDto dto)
    {
        if (!File.Exists(_templatePath))
            throw new FileNotFoundException($"Template not found: {_templatePath}");

        if (!File.Exists(_scriptPath))
            throw new FileNotFoundException($"Python script not found: {_scriptPath}");

        var org = await _settingsService.GetAsync();

        var montantTotal = dto.Droit + dto.PA + dto.PR;
        var montantRestant = dto.Droit * 0.70m;

        var fields = new Dictionary<string, string>
        {
            [F.StampNumber] = Center(dto.DeclarationNumber),
            [F.StampCity] = Center(dto.DeclarationCity),

            // ← Direction et Service depuis les paramètres
            [F.Direction] = Center(org.Diraya),
            [F.Service] = org.Moslaha,

            [F.FullName] = Center(dto.FullName),
            [F.Etablissement] = Center(dto.Etablissement),
            [F.Activite] = Center(dto.Activite),
            [F.NIF] = Center(dto.NIF),
            [F.BP] = Center(dto.BP.ToString()),
            [F.RC] = Center(dto.RC),
            [F.Adresse] = Center(dto.Adresse),
            [F.Telephone] = Center(dto.Telephone),
            [F.Email] = Center(dto.Email),
            [F.QabadhaDaraa] = dto.QabadhaDaraa,
            [F.DateEtatFiscal] = Center(dto.DateEtatFiscal),
            ["Text23"] = string.Empty,
            [F.PeriodeDette] = Center(HardcodedPeriodeDette),
            [F.Droit] = Fmt(dto.Droit),
            [F.PA] = Fmt(dto.PA),
            [F.PR] = Fmt(dto.PR),
            [F.MontantTotal] = Fmt(montantTotal),
            [F.TotalWords] = Center(ArabicWords.Convert(montantTotal), 60),
            [F.MontantRestant] = Fmt(montantRestant),
            [F.MontantRestantWords] = Center(ArabicWords.Convert(montantRestant), 60),
            [F.CheckDafaaWahida] = dto.DafaaWahida ? "/Oui" : "/Off",
            [F.CheckAlaAqsat] = dto.AlaAqsat ? "/Oui" : "/Off",
            [F.SignatureCity] = dto.SignatureCity.Split(' ').Last(),
            [F.SignatureDate] = dto.SignatureDate,
        };

        var outputPath = Path.GetTempFileName() + ".pdf";
        try
        {
            var jsonFields = JsonSerializer.Serialize(fields);
            await RunPythonAsync(_scriptPath, _templatePath, outputPath, jsonFields);
            return await File.ReadAllBytesAsync(outputPath);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    // ── Python runner ─────────────────────────────────────────────────────────

    private static async Task RunPythonAsync(
        string script, string template, string output, string jsonFields)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        psi.Environment["ART122_FIELDS"] = jsonFields;
        psi.ArgumentList.Add(script);
        psi.ArgumentList.Add(template);
        psi.ArgumentList.Add(output);

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start Python process.");

        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"Python script failed (exit {process.ExitCode}): {stderr}");
    }

    // ── Format helpers ────────────────────────────────────────────────────────

    private static string Fmt(decimal v)
    {
        if (v == 0) return string.Empty;
        var rounded = Math.Round(v, 0, MidpointRounding.AwayFromZero);
        var formatted = string.Format("{0:N0}", rounded)
                              .Replace(",", "\u00A0")
                              + ",00";
        return Center(formatted);
    }

    private static string Center(string s, int width = 40) =>
        s.PadLeft((width + s.Length) / 2).PadRight(width);
}

// ─── ARABIC NUMBER TO WORDS ───────────────────────────────────────────────────

public static class ArabicWords
{
    private static readonly string[] Units =
    [
        "", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة",
        "ستة", "سبعة", "ثمانية", "تسعة", "عشرة",
        "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر",
        "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"
    ];

    private static readonly string[] Tens =
    [
        "", "عشرة", "عشرون", "ثلاثون", "أربعون", "خمسون",
        "ستون", "سبعون", "ثمانون", "تسعون"
    ];

    private static readonly string[] Hundreds =
    [
        "", "مئة", "مئتان", "ثلاثمئة", "أربعمئة", "خمسمئة",
        "ستمئة", "سبعمئة", "ثمانمئة", "تسعمئة"
    ];

    public static string Convert(decimal amount)
    {
        amount = Math.Round(amount, 0, MidpointRounding.AwayFromZero);
        if (amount == 0) return "صفر دينار جزائري";
        var intPart = (long)amount;
        return ConvertLong(intPart) + " دينار جزائري";
    }

    private static string ConvertLong(long n)
    {
        if (n == 0) return "صفر";
        var parts = new List<string>();

        if (n >= 1_000_000_000)
        { parts.Add(ConvertInt((int)(n / 1_000_000_000)) + " مليار"); n %= 1_000_000_000; }

        if (n >= 1_000_000)
        {
            var m = n / 1_000_000;
            parts.Add(m == 1 ? "مليون" : m == 2 ? "مليونان" : ConvertInt((int)m) + " ملايين");
            n %= 1_000_000;
        }

        if (n >= 1_000)
        {
            var k = n / 1_000;
            parts.Add(k == 1 ? "ألف" : k == 2 ? "ألفان" : ConvertInt((int)k) + " آلاف");
            n %= 1_000;
        }

        if (n > 0) parts.Add(ConvertInt((int)n));

        return string.Join(" و", parts);
    }

    private static string ConvertInt(int n)
    {
        if (n == 0) return "صفر";
        if (n < 20) return Units[n];

        var parts = new List<string>();

        if (n >= 100) { parts.Add(Hundreds[n / 100]); n %= 100; }

        if (n >= 20)
        {
            var unit = n % 10;
            var ten = n / 10;
            parts.Add(unit > 0 ? Units[unit] + " و" + Tens[ten] : Tens[ten]);
        }
        else if (n > 0)
        {
            parts.Add(Units[n]);
        }

        return string.Join(" و", parts);
    }
}