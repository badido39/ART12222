using ART122.Data;
using Microsoft.EntityFrameworkCore;

namespace ART122.Services;

public class VersementInput
{
    public DateTime DatePrevue { get; set; }
    public decimal Montant { get; set; }
    public string NumeroQuittance { get; set; } = string.Empty;
}

public interface IDeclarationService
{
    Task<List<Declaration>> GetAllAsync();
    Task<Declaration> CreateAsync(
    int redevableId,
    bool dafaaWahida,
    bool alaAqsat,
    DateTime? declarationDate = null,     // ← ajout
    List<VersementInput>? versements = null,
    string? numeroQuittance = null,
    DateTime? datePaiement = null);

    Task DeleteAsync(int id);
    Task<decimal> GetMontantRestantAsync(int redevableId);
    Task<Declaration?> GetByRedevableIdAsync(int redevableId);
    Task AddVersementAsync(int versementId,
                       string numeroQuittance,
                       DateTime datePaiement);
    Task PayVersementAsync(int versementId, string numeroQuittance, DateTime datePaiement);
    Task<List<Declaration>> GetPendingAnnex6Async();
    Task MarkAsSentToAnnex6Async(List<int> ids);

    Task ApplyDGDecisionAsync(
    int declarationId,
    DGDecisionStatus decision,
    DateTime decisionDate,
    string? reference,
    string? note,
    decimal alghaPA,      // amount of PA to cancel (0 if refused)
    decimal alghaPR);     // amount of PR to cancel (0 if refused)


}

public class DeclarationService : IDeclarationService
{
    private readonly ImpotDbContext _db;
    private readonly IRedevableService _redevableService;

    public DeclarationService(ImpotDbContext db, IRedevableService redevableService)
    {
        _db = db;
        _redevableService = redevableService;
    }
    // In DeclarationService
    public async Task<List<Declaration>> GetPendingAnnex6Async()
    {
        return await _db.Declarations
            .Include(d => d.RedevableInfo)
            .Include(d => d.Versements)
            .Where(d => d.Annex6SentAt == null
                   && (
                       // دفعة واحدة → الوصل مدفوع
                       (d.DafaaWahida && d.Versements.All(v => v.EstPaye))
                       ||
                       // على أقساط → جميع الأقساط مدفوعة
                       (d.AlaAqsat && d.Versements.Any() && d.Versements.All(v => v.EstPaye))
                   ))
            .OrderBy(d => d.Number)
            .ToListAsync();
    }
    public async Task MarkAsSentToAnnex6Async(List<int> ids)
    {
        var nextBatch = await _db.Declarations
            .Where(d => d.Annex6BatchNumber.HasValue)
            .MaxAsync(d => (int?)d.Annex6BatchNumber) ?? 0;

        nextBatch++;

        var decls = await _db.Declarations
            .Where(d => ids.Contains(d.Id))
            .ToListAsync();

        var now = DateTime.Now;
        foreach (var d in decls)
        {
            d.Annex6SentAt = now;
            d.Annex6BatchNumber = nextBatch;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<Declaration>> GetAllAsync()
    {
        return await _db.Declarations
            .Include(d => d.RedevableInfo)
            .Include(d => d.Versements.OrderBy(v => v.NumeroOrdre))
            .OrderBy(d => d.Number)
            .ToListAsync();
    }


    public async Task<Declaration?> GetByRedevableIdAsync(int redevableId)
    {
        return await _db.Declarations
            .Include(d => d.Versements)
            .Include(d => d.RedevableInfo)
            .FirstOrDefaultAsync(d => d.RedevableInfoId == redevableId);
    }

    public async Task PayVersementAsync(int versementId, string numeroQuittance, DateTime datePaiement)
    {
        var versement = await _db.Versements.FindAsync(versementId);

        if (versement is null)
            throw new InvalidOperationException("Versement introuvable.");

        if (versement.EstPaye)
            throw new InvalidOperationException("Ce versement est déjà payé.");

        versement.NumeroQuittance = numeroQuittance.Trim();
        versement.DatePaiement = datePaiement;
        versement.EstPaye = true;
        versement.Status = VersementStatus.Paye;

        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetMontantRestantAsync(int redevableId)
    {
        var summaries = await _redevableService.GetTaxSummaryAsync();
        var summary = summaries.FirstOrDefault(s => s.RedevableId == redevableId);
        if (summary is null) return 0;
        return Math.Round(summary.TotalDroit * 0.70m, 0, MidpointRounding.AwayFromZero);
    }

    public async Task<Declaration> CreateAsync(
     int redevableId,
     bool dafaaWahida,
     bool alaAqsat,
     DateTime? declarationDate = null,     // ← ajout
     List<VersementInput>? versements = null,
     string? numeroQuittance = null,
     DateTime? datePaiement = null)
    {
        var existing = await _db.Declarations
            .FirstOrDefaultAsync(d => d.RedevableInfoId == redevableId);

        if (existing is not null)
            throw new InvalidOperationException(
                "هذا المكلف مسجل مسبقاً في السجل برقم التصريح: " + existing.Number);

        var summaries = await _redevableService.GetTaxSummaryAsync();
        var summary = summaries.FirstOrDefault(s => s.RedevableId == redevableId)
            ?? throw new InvalidOperationException($"Redevable {redevableId} not found.");

        var droit = summary.TotalDroit;
        var pa = summary.TotalPA;
        var pr = summary.TotalPR;
        var montantTotal = droit + pa + pr;
        var montantRestant = Math.Round(droit * 0.70m, 0, MidpointRounding.AwayFromZero);

        var nextNumber = await _db.Declarations.AnyAsync()
            ? await _db.Declarations.MaxAsync(d => d.Number) + 1
            : 1;

        var declaration = new Declaration
        {
            Number = nextNumber,
            RedevableInfoId = redevableId,
            DafaaWahida = dafaaWahida,
            AlaAqsat = alaAqsat,
            Date = declarationDate?.Date ?? DateTime.Today,  // ← fix

            NumeroQuittance = numeroQuittance,   // ← map to entity
            DatePaiement = datePaiement,      // ← map to entity
            Droit = droit,
            PA = pa,
            PR = pr,
            MontantTotal = montantTotal,
            MontantRestant = montantRestant,
            Versements = new List<Versement>()
        };

        if (dafaaWahida)
        {
            // Single payment — create one versement already marked as paid
            declaration.Versements.Add(new Versement
            {
                NumeroOrdre = 1,
                DatePrevue = datePaiement ?? DateTime.Today,
                DatePaiement = datePaiement ?? DateTime.Today,
                Montant = montantRestant,
                NumeroQuittance = numeroQuittance ?? string.Empty,
                EstPaye = !string.IsNullOrWhiteSpace(numeroQuittance),
                Status = !string.IsNullOrWhiteSpace(numeroQuittance)
                                       ? VersementStatus.Paye
                                       : VersementStatus.EnAttente,
            });
        }
        else if (alaAqsat && versements is { Count: > 0 })
        {
            for (int i = 0; i < versements.Count; i++)
            {
                declaration.Versements.Add(new Versement
                {
                    NumeroOrdre = i + 1,
                    DatePrevue = versements[i].DatePrevue,
                    Montant = versements[i].Montant,
                    NumeroQuittance = versements[i].NumeroQuittance,
                    EstPaye = false,
                    Status = VersementStatus.EnAttente,
                });
            }
        }

        _db.Declarations.Add(declaration);
        await _db.SaveChangesAsync();
        return declaration;
    }

    public async Task DeleteAsync(int id)
    {
        var d = await _db.Declarations.FindAsync(id);
        if (d is not null) { _db.Declarations.Remove(d); await _db.SaveChangesAsync(); }
    }

    public Task AddVersementAsync(int versementId, string numeroQuittance, DateTime datePaiement)
    {
        throw new NotImplementedException();
    }
    public async Task ApplyDGDecisionAsync(
    int declarationId,
    DGDecisionStatus decision,
    DateTime decisionDate,
    string? reference,
    string? note,
    decimal alghaPA,
    decimal alghaPR)
    {
        var decl = await _db.Declarations.FindAsync(declarationId)
            ?? throw new InvalidOperationException("Déclaration introuvable.");

        decl.DGDecision = decision;
        decl.DGDecisionDate = decisionDate;
        decl.DGDecisionReference = reference?.Trim();
        decl.DGDecisionNote = note?.Trim();

        if (decision == DGDecisionStatus.Acceptee)
        {
            // Cap to what was declared — DG can't cancel more than originally declared
            decl.AlghaPA = Math.Min(alghaPA, decl.PA);
            decl.AlghaPR = Math.Min(alghaPR, decl.PR);
        }
        else
        {
            // Refused → reset any previously set cancellations
            decl.AlghaPA = 0;
            decl.AlghaPR = 0;
        }

        await _db.SaveChangesAsync();
    }
}