using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BonTemps.Models
{
    public class Reservering
    {
        public int Id { get; set; }
        public virtual ApplicationUser Klant { get; set; }
        
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Datum { get; set; } = DateTime.Now.Date;

        public DateTime? StartTijd { get; set; }
        public DateTime? EindTijd { get; set; }

        public virtual ICollection<Menu> BesteldeMenus { get; set; }
    }
}