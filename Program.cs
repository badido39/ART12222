using ART122.Components;
using ART122.Data;
using ART122.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


// SQLite connection
builder.Services.AddDbContext<ImpotDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);




builder.Services.AddScoped<IRedevableService, RedevableService>();
builder.Services.AddScoped<IImpotService, ImpotService>();
builder.Services.AddScoped<INatureImpotService, NatureImpotService>();
builder.Services.AddScoped<IArt122PdfService, Art122PdfService>();

builder.Services.AddScoped<IDeclarationService, DeclarationService>();
builder.Services.AddScoped<IAppSettingsService, AppSettingsService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();



 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ImpotDbContext>();

    db.Database.EnsureCreated(); // or Migrate()
                                 // =========================
                                 // SEED NATURE IMPOT
                                 // =========================
    if (!db.NatureImpots.Any())
    {
        db.NatureImpots.AddRange(
            new NatureImpot { Name = "TVA" },
            new NatureImpot { Name = "TAP" },
            new NatureImpot { Name = "IBS" },
            new NatureImpot { Name = "IRG" },
            new NatureImpot { Name = "TAIC" },
            new NatureImpot { Name = "Timbre" }
        );

        db.SaveChanges();
    }

}

app.Run();
