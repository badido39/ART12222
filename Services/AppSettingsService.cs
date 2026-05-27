using ART122.Data;

namespace ART122.Services
{
    public interface IAppSettingsService
    {
        Task<AppSettings> GetAsync();
        Task SaveAsync(AppSettings settings);
    }

    public class AppSettingsService : IAppSettingsService
    {
        private readonly ImpotDbContext _db;

        public AppSettingsService(ImpotDbContext db) => _db = db;

        public async Task<AppSettings> GetAsync()
        {
            var s = await _db.AppSettings.FindAsync(1);
            if (s is null)
            {
                s = new AppSettings();
                _db.AppSettings.Add(s);
                await _db.SaveChangesAsync();
            }
            return s;
        }

        public async Task SaveAsync(AppSettings settings)
        {
            settings.Id = 1;
            var existing = await _db.AppSettings.FindAsync(1);
            if (existing is null)
                _db.AppSettings.Add(settings);
            else
            {
                existing.Diraya = settings.Diraya;
                existing.Qabada = settings.Qabada;
                existing.Moslaha = settings.Moslaha;
            }
            await _db.SaveChangesAsync();
        }
    }
}
