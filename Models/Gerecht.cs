using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BonTemps.Models
{
    public class Gerecht
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public GerechtSoort Soort { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<Ingredient> Ingredients { get; set; }
    }
    public enum GerechtSoort { Voorgerecht, Hoofdgerecht, Nagerecht}
}