using Microsoft.EntityFrameworkCore;
using Lemmikkitietokanta.Data;
using Lemmikkitietokanta.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Määritetään tietokanta käyttämään SQLite-tietokantaa
builder.Services.AddDbContext<LemmikkiDbContext>(opt =>
    opt.UseSqlite("Data Source=../Lemmikkitietokanta/Lemmikkitietokanta.db"),
    ServiceLifetime.Scoped); // Määritetään elinkaari "Scoped"

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Lisää logger-palvelu
builder.Services.AddLogging();

// Swagger ja dokumentointi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "LemmikkiAPI";
    config.Title = "Lemmikkitietokanta API";
    config.Version = "v1";
    config.Description = "API lemmikkien ja omistajien hallintaan";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Lemmikkitietokanta API";
        config.Path = "/api-docs";
        config.DocumentPath = "/api-docs/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

// API-päätepisteet lemmikkien hallintaan
app.MapPost("/lemmikit", async (Lemmikki uusiLemmikki, LemmikkiDbContext db) =>
{
    var omistaja = await db.Omistajat.FindAsync(uusiLemmikki.OmistajaId);
    if (omistaja == null)
    {
        return Results.BadRequest("Annettu OmistajaId ei ole olemassa.");
    }

    db.Lemmikit.Add(uusiLemmikki);
    await db.SaveChangesAsync();
    return Results.Created($"/lemmikit/{uusiLemmikki.Id}", uusiLemmikki);
});

app.MapGet("/lemmikit", async (LemmikkiDbContext db) =>
    await db.Lemmikit
            .Include(l => l.Omistaja) // Liittää omistajan tiedot mukaan
            .ToListAsync());

app.MapGet("/lemmikit/{id}", async (int id, LemmikkiDbContext db) =>
    await db.Lemmikit.FindAsync(id)
        is Lemmikki lemmikki
            ? Results.Ok(lemmikki)
            : Results.NotFound());

app.MapGet("/lemmikit/rotu/{rotu}", async (string rotu, LemmikkiDbContext db) =>
    await db.Lemmikit.Where(l => l.Rotu == rotu).ToListAsync());

app.MapPut("/lemmikit/{id}", async (int id, Lemmikki inputLemmikki, LemmikkiDbContext db) =>
{
    var lemmikki = await db.Lemmikit.FindAsync(id);

    if (lemmikki is null) return Results.NotFound();

    lemmikki.Nimi = inputLemmikki.Nimi;
    lemmikki.Rotu = inputLemmikki.Rotu;
    lemmikki.OmistajaId = inputLemmikki.OmistajaId;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/lemmikit/{id}", async (int id, LemmikkiDbContext db) =>
{
    if (await db.Lemmikit.FindAsync(id) is Lemmikki lemmikki)
    {
        db.Lemmikit.Remove(lemmikki);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

// API-päätepisteet omistajien hallintaan
app.MapGet("/omistajat", async (LemmikkiDbContext db) =>
    await db.Omistajat.ToListAsync());

app.MapGet("/omistajat/{id}", async (int id, LemmikkiDbContext db) =>
    await db.Omistajat.FindAsync(id)
        is Omistaja omistaja
            ? Results.Ok(omistaja)
            : Results.NotFound());

app.MapPost("/omistajat", async (Omistaja omistaja, LemmikkiDbContext db) =>
{
    db.Omistajat.Add(omistaja);
    await db.SaveChangesAsync();

    return Results.Created($"/omistajat/{omistaja.Id}", omistaja);
});

app.MapPut("/omistajat/{id}", async (int id, Omistaja inputOmistaja, LemmikkiDbContext db) =>
{
    var omistaja = await db.Omistajat.FindAsync(id);

    if (omistaja is null) return Results.NotFound();

    omistaja.Nimi = inputOmistaja.Nimi;
    omistaja.Puhelin = inputOmistaja.Puhelin;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/omistajat/{id}", async (int id, LemmikkiDbContext db) =>
{
    if (await db.Omistajat.FindAsync(id) is Omistaja omistaja)
    {
        db.Omistajat.Remove(omistaja);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();