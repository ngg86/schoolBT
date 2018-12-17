using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BonTemps.Models
{
    //Net als couvert maar dan voor gerecht en ingredient
    public class TotalIngredients
    {
        public int Id { get; set; }

        [Index("IX MenuReservering", IsUnique = true, Order = 1)]
        public virtual Gerecht Gerecht { get; set; }
        [Index("IX MenuReservering", IsUnique = true, Order = 2)]
        public virtual Ingredient Ingredient { get; set; }

        public int Aantal { get; set; }
    }
}