using ART122.Data;
using Microsoft.EntityFrameworkCore;
using System;
using static ART122.Data.DTOs;

namespace ART122.Services;

public interface IRedevableService
{
    Task<List<RedevableInfo>> GetAllAsync();
    Task CreateFullAsync(RedevableCreateDto dto);
    Task<RedevableInfo> Create(RedevableCreateDto dto);
    Task AddImpots(int redevableId, List<ImpotCreateDto> impots); // ✅ ADD THIS
    Task<ValidationResult> ValidateRedevable(int bp, int nif, int article);
    Task UpdateAsync(RedevableInfo entity);
    Task<ValidationResult> DeleteAsync(int id);
    Task<List<RedevableTaxSummaryDto>> GetTaxSummaryAsync();
    Task<RedevableInfo?> GetByIdAsync(int id);

}

public class RedevableService : IRedevableService
{
    private readonly ImpotDbContext _db;

    public RedevableService(ImpotDbContext db)
    {
        _db = db;
    }

    public async Task<RedevableInfo?> GetByIdAsync(int id)
    {
        return await _db.Redevables
            .Include(x => x.Impots)
                .ThenInclude(i => i.NatureImpot)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task<List<RedevableTaxSummaryDto>> GetTaxSummaryAsync()
    {
        return await _db.Redevables
            .Include(r => r.Impots)
            .Select(r => new RedevableTaxSummaryDto
            {
                RedevableId = r.Id,
                BP = r.BP,
                FullName = r.FullName,

                TotalPA = r.Impots.Sum(i => i.PA),
                TotalPR = r.Impots.Sum(i => i.PR),
                TotalDroit = r.Impots.Sum(i => i.Droit)
            })
            .ToListAsync();
    }
    public async Task<ValidationResult> DeleteAsync(int id)
    {
        var redevable = await _db.Redevables
            .Include(r => r.Impots)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (redevable == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Redevable not found"
            };
        }

        if (redevable.Impots.Any())
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Cannot delete: this redevable has existing impots"
            };
        }

        _db.Redevables.Remove(redevable);
        await _db.SaveChangesAsync();

        return new ValidationResult
        {
            IsValid = true,
            Message = "Deleted successfully"
        };
    }
    public async Task<List<RedevableInfo>> GetAllAsync()
    {
        return await _db.Redevables
            .Include(r => r.Impots)
                .ThenInclude(i => i.NatureImpot)
            .ToListAsync();
    }
    public async Task CreateFullAsync(RedevableCreateDto dto)
    {
        var redevable = new RedevableInfo
        {
            BP = dto.BP,  // ✅ NEW
            FullName = dto.FullName,
            FilsDe = dto.FilsDe,
            Adresse = dto.Adresse,
            Article = dto.Article,
            Telephone = dto.Telephone,
            Etablissement = dto.Etablissement,
            NIF = dto.NIF,
            Email = dto.Email,
            Activite = dto.Activite,
            Impots = new List<Impot>()
        };

        foreach (var imp in dto.Impots)
        {
            redevable.Impots.Add(new Impot
            {
                RoleNumber = imp.RoleNumber,
                YearImpot = imp.YearImpot,  // int now
                PA = imp.PA,
                PR = imp.PR,
                NatureImpotId = imp.NatureImpotId
            });
        }

        _db.Redevables.Add(redevable);
        await _db.SaveChangesAsync();
    }
    public async Task<RedevableInfo> Create(RedevableCreateDto dto)
    {
        var entity = new RedevableInfo
        {
            BP = dto.BP,
            FullName = dto.FullName,
            FilsDe = dto.FilsDe,
            Adresse = dto.Adresse,
            Article = dto.Article,
            Telephone = dto.Telephone,
            Email = dto.Email,
            Activite = dto.Activite,
            NIF = dto.NIF,
            NumExtraitDeRole = dto.NumExtraitDeRole,
            DateExtraitDeRole = dto.DateExtraitDeRole,
            Etablissement = dto.Etablissement,
            DateDeclaration = dto.DateDeclaration,
        };

        _db.Redevables.Add(entity);
        await _db.SaveChangesAsync();

        return entity;
    }
    public async Task AddImpots(int redevableId, List<ImpotCreateDto> impots)
    {
        var redevable = await _db.Redevables
            .Include(r => r.Impots)
            .FirstOrDefaultAsync(r => r.Id == redevableId);

        if (redevable == null)
            throw new Exception("Redevable not found");

        foreach (var i in impots)
        {
            redevable.Impots.Add(new Impot
            {
                RoleNumber = i.RoleNumber,
                PA = i.PA,
                PR = i.PR,
                Droit = i.Droit,
                YearImpot = i.YearImpot,
                NatureImpotId = i.NatureImpotId,
                RedevableInfoId = redevableId
            });
        }

        await _db.SaveChangesAsync();
    }
    public async Task<ValidationResult> ValidateRedevable(int bp, int nif, int article)
    {
        var exists = await _db.Redevables
            .AnyAsync(r =>
                r.BP == bp ||
                r.NIF == nif ||
                r.Article == article);

        if (exists)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "BP, NIF or Article already exists"
            };
        }

        return new ValidationResult
        {
            IsValid = true
        };
    }

    public async Task UpdateAsync(RedevableInfo entity)
    {
        var dbEntity = await _db.Redevables.FindAsync(entity.Id);

        if (dbEntity == null)
            return;

        dbEntity.BP = entity.BP;
        dbEntity.Article = entity.Article;
        dbEntity.NIF = entity.NIF;
        dbEntity.FullName = entity.FullName;
        dbEntity.Adresse = entity.Adresse;
        dbEntity.Telephone = entity.Telephone;
        dbEntity.Email = entity.Email;
        dbEntity.Activite = entity.Activite;
        dbEntity.Etablissement = entity.Etablissement;

        await _db.SaveChangesAsync();
    }
}

public interface IImpotService
{
    Task<List<Impot>> GetAllAsync();
    Task AddAsync(Impot impot);
    Task DeleteAsync(int id);
    Task UpdateAsync(Impot impot);


}
public class ImpotService : IImpotService
{
    private readonly ImpotDbContext _db;

    public ImpotService(ImpotDbContext db)
    {
        _db = db;
    }

    public async Task<List<Impot>> GetAllAsync()
    {
        return await _db.Impots
            .Include(i => i.RedevableInfo)
            .Include(i => i.NatureImpot)
            .ToListAsync();
    }

    public async Task AddAsync(Impot impot)
    {
        _db.Impots.Add(impot);
        await _db.SaveChangesAsync();
    }
    public async Task DeleteAsync(int id)
    {
        var impot = await _db.Impots.FindAsync(id);
        if (impot != null)
        {
            _db.Impots.Remove(impot);
            await _db.SaveChangesAsync();
        }
    }
    public async Task UpdateAsync(Impot impot)
    {
        var dbEntity = await _db.Impots
            .FirstOrDefaultAsync(x => x.Id == impot.Id);

        if (dbEntity == null)
            return;

        dbEntity.RoleNumber = impot.RoleNumber;
        dbEntity.YearImpot = impot.YearImpot;
        dbEntity.PA = impot.PA;
        dbEntity.PR = impot.PR;
        dbEntity.Droit = impot.Droit;
        dbEntity.NatureImpotId = impot.NatureImpotId;

        await _db.SaveChangesAsync();
    }


}
public interface INatureImpotService
{
    Task<List<NatureImpot>> GetAllAsync();
    Task<NatureImpot> CreateAsync(NatureImpot entity);
    Task UpdateAsync(NatureImpot entity);
    Task DeleteAsync(int id);
}

public class NatureImpotService : INatureImpotService
{
    private readonly ImpotDbContext _db;

    public NatureImpotService(ImpotDbContext db)
    {
        _db = db;
    }

    public async Task<List<NatureImpot>> GetAllAsync()
    {
        return await _db.NatureImpots.ToListAsync();
    }
    public async Task<NatureImpot> CreateAsync(NatureImpot entity)
    {
        _db.NatureImpots.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(NatureImpot entity)
    {
        var dbEntity = await _db.NatureImpots.FindAsync(entity.Id);

        if (dbEntity == null)
            return;

        dbEntity.Name = entity.Name;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.NatureImpots.FindAsync(id);
        if (item != null)
        {
            _db.NatureImpots.Remove(item);
            await _db.SaveChangesAsync();
        }
    }
}
