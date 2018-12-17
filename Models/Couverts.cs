using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BonTemps.Models
{
    public class Couverts
    {
        public int Id { get; set; }

        [Index("IX MenuReservering", IsUnique = true, Order = 1)]
        public virtual Reservering Reservering { get; set; }
        [Index("IX MenuReservering", IsUnique = true, Order = 2)]
        public virtual Menu Menu { get; set; }

        public int Aantal { get; set; }
    }
}