using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

//Hier zijn alle viewmodels wat ik heb gebruikt
//Sommige zijn waarschijnlijk overbodig en niet nodig

namespace BonTemps.Models
{
    public class ViewModels
    {
        public class GerechtMenuViewModel
        {
            public Menu Menu { get; set; }
            public Gerecht Gerecht { get; set; }
            
            public List<Menu> MenuList { get; set; }
            public List<Gerecht> GerechtList { get; set; }
        }

        public class ReserveringViewModel
        {
            public List<string> Times { get; set; }
            public Menu Menu { get; set; }
            public List<Menu> Menus { get; set; }
            public int Total { get; set; }
            public int ReserveringID { get; set; }
            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
            public DateTime SelectedDate { get; set; }
            public long SelectedHour { get; set; }
            public Reservering Reservering { get; set; }
        }

        public class InfoViewModel
        {
            public int Aantal { get; set; }
            public List<Ingredient> Ingredients { get; set; }
            public DateTime Date { get; set; }
        }

        public class ReserveringCouvertViewMmodel
        {
            public Reservering Reservering {get;set;}
            public List<Reservering> Reserveringen { get; set; }
            public ICollection<Couverts> Couverts { get; set; }
        }
        public class ShowInfoViewModel
        {
            public Dictionary<DateTime, DateTime> DatesAndTimes { get; set; }
            //list of reservering and menus (couvert) ordered on that day
            public List<Couverts> Couverts { get; set; }
            public Dictionary<string, int> IngredientTotal { get; set; }
        }

        public class TotalIngredientsGerechtViewModel
        {
            public Gerecht Gerecht { get; set; }
            public List<Gerecht> Gerechts { get; set; }
            public ICollection<TotalIngredients> TotalIngredients { get; set; }
        }

        public class IngredientGerechtViewModel
        {
            public int GerechtId { get; set; }
            public Gerecht Gerecht { get; set; }
            public Ingredient Ingredient { get; set; }
            public int Total { get; set; }
            public List<Ingredient> Ingredients { get; set; }
        }
    }
}