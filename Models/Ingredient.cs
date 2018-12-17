using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BonTemps.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Eenheid { get; set; }

        [NotMapped]
        public string NaamMetEenheid { get { return Naam + " - " + Eenheid; } }

        public virtual ICollection<Gerecht> Gerechten { get; set; }
    }
}