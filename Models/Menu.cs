using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BonTemps.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Omschrijving { get; set; }
        public decimal Prijs { get; set; }
        public virtual ICollection<Gerecht> Gerechten { get; set; }
        public virtual ICollection<Reservering> Reserverings { get; set; }
    }
}