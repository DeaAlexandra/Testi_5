using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Lemmikkitietokanta.Data;
using Lemmikkitietokanta.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LemmikkitietokantaSovellus.Pages
{
    public class OmistajatJaLemmikitModel : PageModel
    {
        private readonly LemmikkiDbContext _context;
        private readonly ILogger<OmistajatJaLemmikitModel> _logger;

        public OmistajatJaLemmikitModel(LemmikkiDbContext context, ILogger<OmistajatJaLemmikitModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // BindProperty ominaisuudet ovat erittäin tärkeitä ja jokaisella pitää olla oma BindProperty, jos ei ole niin ongelmia tulee
        [BindProperty]
        public Omistaja NewOmistaja { get; set; } = new Omistaja(); // Lisää Omistaja

        [BindProperty]
        public Lemmikki NewLemmikki { get; set; } = new Lemmikki(); // Lisää Lemmikki
        [BindProperty]
        public string OmistajaNimi { get; set; } // Omistajan nimi lemmikin lisäystä varten

        [BindProperty]
        public int? EtsittyOmistajaId { get; set; } // Etsitty omistajan ID

        [BindProperty]
        public int? EtsiOmistajanId { get; set; } // Etsittävä omistajan ID

        [BindProperty]
        public List<Lemmikki> EtsitytLemmikit { get; set; } = new List<Lemmikki>(); // Etsityt lemmikit

        [BindProperty]
        public Omistaja EtsittyOmistaja { get; set; } // Etsitty omistaja

        public List<Omistaja> Omistajat { get; set; } = new List<Omistaja>();
        public List<Lemmikki> Lemmikit { get; set; } = new List<Lemmikki>();

        public async Task<IActionResult> OnGetAsync()
        {
            Omistajat = await _context.Omistajat.ToListAsync(); // Fetch owners
            Lemmikit = await _context.Lemmikit.Include(l => l.Omistaja).ToListAsync(); // Fetch pets with their owners

            return Page(); // Return the page result to render the view
        }

        public async Task<IActionResult> OnPostAddOmistajaAsync()
        {
            ModelState.Clear(); // Clear previous ModelState errors

            if (!TryValidateModel(NewOmistaja, nameof(NewOmistaja)))
            {
                _logger.LogWarning("ModelState is invalid.");
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        _logger.LogWarning("Key: {Key}, Error: {Error}", modelStateKey, error.ErrorMessage);
                    }
                }
                return Page();
            }

            _context.Omistajat.Add(NewOmistaja);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Omistaja added successfully: {Omistaja}", NewOmistaja);
            return RedirectToPage();
        }

        // Poista Omistaja, ei voi poistaa mikäli omistajalla on lemmikkejä
        public async Task<IActionResult> OnPostDeleteOmistajaAsync(int id)
        {
            var omistaja = await _context.Omistajat
                .Include(o => o.Lemmikit) // Include related Lemmikit
                .FirstOrDefaultAsync(o => o.Id == id);

            if (omistaja != null)
            {
                if (omistaja.Lemmikit.Any())
                {
                    TempData["ErrorMessage"] = "Et voi poistaa omistajaa, jolla on vielä lemmikkejä tietokannassa.";
                    return RedirectToPage();
                }

                _context.Omistajat.Remove(omistaja);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        //lemmikin lisäys
        public async Task<IActionResult> OnPostAddLemmikkiAsync()
        {
            try
            {
                ModelState.Clear(); // Clear previous ModelState errors

                if (!TryValidateModel(NewLemmikki, nameof(NewLemmikki)))
                {
                    _logger.LogWarning("ModelState is invalid.");
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            _logger.LogWarning("Key: {Key}, Error: {Error}", modelStateKey, error.ErrorMessage);
                        }
                    }
                    // Add logic to fetch the owners again if ModelState is invalid
                    Omistajat = await _context.Omistajat.ToListAsync();
                    return Page(); // Return to the same page with validation errors
                }

                // Check if OmistajaId is valid
                var omistaja = await _context.Omistajat.FindAsync(NewLemmikki.OmistajaId);
                if (omistaja == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid OmistajaId");
                    _logger.LogWarning("Invalid OmistajaId: {OmistajaId}", NewLemmikki.OmistajaId);
                    Omistajat = await _context.Omistajat.ToListAsync();
                    return Page();
                }

                _context.Lemmikit.Add(NewLemmikki);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lemmikki added successfully: {Lemmikki}", NewLemmikki);
                return RedirectToPage(); // Redirect after successful addition
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error adding pet: {ex.Message}");
                _logger.LogError(ex, "Error adding pet");
                Omistajat = await _context.Omistajat.ToListAsync();
                return Page();
            }
        }

        //Etsi omistaja nimen perusteella
        public async Task<IActionResult> OnPostEtsiOmistajaAsync()
        {
            var omistaja = await _context.Omistajat.FirstOrDefaultAsync(o => o.Nimi == OmistajaNimi);
            if (omistaja == null)
            {
                ModelState.AddModelError(string.Empty, "Omistajaa ei löytynyt annetulla nimellä");
                _logger.LogWarning("Omistajaa ei löytynyt annetulla nimellä: {OmistajaNimi}", OmistajaNimi);
                Omistajat = await _context.Omistajat.ToListAsync();
                return Page();
            }

            EtsittyOmistajaId = omistaja.Id;
            _logger.LogInformation("Omistaja löytyi: {OmistajaId}", EtsittyOmistajaId);

            Omistajat = await _context.Omistajat.ToListAsync();
            Lemmikit = await _context.Lemmikit.Include(l => l.Omistaja).ToListAsync();
            return Page();
        }

        //Etsi lemmikit omistajan ID:n perusteella
        public async Task<IActionResult> OnPostEtsiLemmikitAsync()
        {
            if (EtsiOmistajanId.HasValue)
            {
                EtsitytLemmikit = await _context.Lemmikit
                    .Where(l => l.OmistajaId == EtsiOmistajanId.Value)
                    .Include(l => l.Omistaja)
                    .ToListAsync();
                _logger.LogInformation("Löydettiin {Count} lemmikkiä omistajan ID:llä {OmistajaId}", EtsitytLemmikit.Count, EtsiOmistajanId);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Omistajan ID on pakollinen");
                _logger.LogWarning("Omistajan ID on pakollinen");
            }

            Omistajat = await _context.Omistajat.ToListAsync();
            Lemmikit = await _context.Lemmikit.Include(l => l.Omistaja).ToListAsync();
            return Page();
        }

        //Etsi omistaja ID:n perusteella
        public async Task<IActionResult> OnPostEtsiOmistajaNimellaAsync()
        {
            if (EtsiOmistajanId.HasValue)
            {
                EtsittyOmistaja = await _context.Omistajat.FindAsync(EtsiOmistajanId.Value);
                if (EtsittyOmistaja == null)
                {
                    ModelState.AddModelError(string.Empty, "Omistajaa ei löytynyt annetulla ID:llä");
                    _logger.LogWarning("Omistajaa ei löytynyt annetulla ID:llä: {OmistajaId}", EtsiOmistajanId);
                }
                else
                {
                    _logger.LogInformation("Omistaja löytyi: {Omistaja}", EtsittyOmistaja);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Omistajan ID on pakollinen");
                _logger.LogWarning("Omistajan ID on pakollinen");
            }

            Omistajat = await _context.Omistajat.ToListAsync();
            Lemmikit = await _context.Lemmikit.Include(l => l.Omistaja).ToListAsync();
            return Page();
        }


        // Poista lemmikki
        public async Task<IActionResult> OnPostDeleteLemmikkiAsync(int id)
        {
            var lemmikki = await _context.Lemmikit.FindAsync(id);
            if (lemmikki != null)
            {
                _context.Lemmikit.Remove(lemmikki);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}