using Lemmikkitietokanta.Models;
using Microsoft.EntityFrameworkCore;


// Tiedosto: Models/Omistaja.cs


namespace Lemmikkitietokanta.Models
{
    public class Omistaja
    {
        public int Id { get; set; }
        public string Nimi { get; set; }
        public string Puhelin { get; set; }

        public List<Lemmikki>? Lemmikit { get; set; } // Navigaatio-ominaisuus
    }
}
