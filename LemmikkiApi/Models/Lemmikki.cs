using Lemmikkitietokanta.Models;
using Microsoft.EntityFrameworkCore;


namespace Lemmikkitietokanta.Models
{
    public class Lemmikki
    {
        public int Id { get; set; }
        public string Nimi { get; set; }
        public string Rotu { get; set; }

        public int OmistajaId { get; set; } // Viite Omistaja-tauluun
        public Omistaja? Omistaja { get; set; } // Navigaatio-ominaisuus
    }
}