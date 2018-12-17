using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BonTemps.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using static BonTemps.Models.ViewModels;

namespace BonTemps.Controllers
{
    [Authorize(Roles = "Admin, Manager, Client")]
    public class ReserveringsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        bool _available = true;
        //totaal beschikbare stoelen - handiger om het in een instelling formulier op te slaan
        //dan is het makkelijker om aan te passen
        const int _maxSeats = 50;

        // GET: Reserverings
        public ActionResult Index()
        {
            var id = User.Identity.GetUserId();

            List<Reservering> reserveringen = db.Reserverings.Where(r => r.Klant.Id == id).ToList();
            List<Couverts> couverts = db.Couverts.Where(c => c.Reservering.Klant.Id == id).ToList();
            ReserveringCouvertViewMmodel rcvm = new ReserveringCouvertViewMmodel
            {
                Reserveringen = reserveringen,
                Couverts = couverts
            };
            return View(rcvm);
        }

        // GET: Reserverings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservering reservering = db.Reserverings.Find(id);
            if (reservering == null)
            {
                return HttpNotFound();
            }
            return View(reservering);
        }

        // GET: Reserverings/Create
        [HttpGet]
        public ActionResult Create()
        {
            var resVM = new ReserveringViewModel
            {
                Reservering = new Reservering(),
                SelectedDate = DateTime.Now.Date
            };
            return View(resVM);
        }

        // POST: Reserverings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //([Bind(Include = "Id,Datum,StartTijd,EindTijd")] Reservering reservering)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModels.ReserveringViewModel mrViewmodel)
        {
            var currentUser = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                Reservering reservering = new Reservering();
                DateTime reservedDate = mrViewmodel.SelectedDate;

                reservering.StartTijd = reservedDate.AddHours(mrViewmodel.SelectedHour);
                reservering.Klant = db.Users.Where(u => u.Id == currentUser).FirstOrDefault();
                reservering.Datum = mrViewmodel.SelectedDate;

                //eindtijd van een reservering is standaard 2uur later dan de begintijd
                reservering.EindTijd = reservering.StartTijd.Value.AddMinutes(120);

                db.Reserverings.Add(reservering);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(mrViewmodel.Reservering);
        }

        //Send client to AddMenu view with Id of the selected reservering
        public ActionResult AddMenu(Reservering reservering)
        {
            if (reservering.Id != 0)
            {
                reservering = db.Reserverings.Where(i => i.Id == reservering.Id).FirstOrDefault();

                ReserveringViewModel rmViewModel = new ReserveringViewModel()
                {
                    Reservering = reservering,
                    //toon alleen menus met gerechten
                    //dit voorkomt dat er lege menus op de view komen te staan.
                    Menus = db.Menus.Where(a => a.Gerechten.Count != 0).ToList()
                };
                return View(rmViewModel);
            }
            return RedirectToAction("Index");
        }

        //Voeg een menu, en de aantal daarvan, toe aan een reservering
        [HttpPost]
        public ActionResult MenuToReservering(ViewModels.ReserveringViewModel model)
        {
            if (ModelState.IsValid)
            {
                //voordat een reservering komt in de database te staan
                //moeten we eerst checken of er genoeg beschikbare stoelen zijn
                //voor de gekozen datum en tijd
                _available = CheckSeatAvailability(model);
                if (!_available)
                {
                    //een foutmelding is teruggestuurd, kan mooier
                    TempData["ErrorMessage"] = "Helaas zijn wij volgeboekt op de door uw gekozen datum en tijd !";
                    return RedirectToAction("Index");
                }

                Reservering reservering = db.Reserverings.Find(model.Reservering.Id);
                List<Menu> orderedMenus = reservering.BesteldeMenus.ToList();

                //checken of de menu al besteld is
                //zo niet, maken we een nieuwe couvert aan
                Menu existingMenu = orderedMenus.Where(m => m.Id == model.Menu.Id).FirstOrDefault();
                if (existingMenu == null)
                {
                    Menu menu = db.Menus.Find(model.Menu.Id);

                    Couverts cv = new Couverts
                    {
                        Menu = menu,
                        Aantal = model.Total,
                        Reservering = reservering
                    };

                    reservering.BesteldeMenus.Add(menu);
                    db.Couverts.Add(cv);
                    db.Entry(reservering).State = EntityState.Modified;
                    db.SaveChanges();
                }
                //als de menu al besteld is met deze reservering
                //geven we alleen een nieuwe aantal aan
                //Ik heb voor deze manier gekozen omdat het eenvoudiger is om
                //een nieuw aantal aan te geven ipv toe te voegen of weg te halen.
                else
                {
                    var cou = db.Couverts.Where(c => c.Menu.Id == existingMenu.Id).FirstOrDefault();
                    if (cou != null)
                    {
                        cou.Aantal = model.Total;
                        db.Entry(cou).State = EntityState.Modified;

                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        private bool CheckSeatAvailability(ViewModels.ReserveringViewModel model)
        {
            var cvList = db.Couverts.Where(c => c.Reservering.Datum == model.Reservering.Datum && c.Reservering.StartTijd == model.Reservering.StartTijd).ToList();
            int totalSeats = 0;
            foreach (var couvert in cvList)
            {
                //checken of reservering dit menu al heeft besteld
                if (couvert.Reservering.Id == model.Reservering.Id && couvert.Menu.Id == model.Menu.Id)
                {
                    //tel de huidige couvert aantal niet
                    totalSeats += model.Total; continue;
                }
                totalSeats += couvert.Aantal;
                if (totalSeats + model.Total > _maxSeats) return false;
            }
            if (totalSeats > _maxSeats) return false;
            return true;
        }

        // GET: Reserverings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReserveringViewModel reserveringvm = new ReserveringViewModel
            {
                Reservering = db.Reserverings.Find(id),
            };

            if (reserveringvm.Reservering != null)
            {
                reserveringvm.ReserveringID = (int)id;
                reserveringvm.SelectedDate = reserveringvm.Reservering.Datum;
                reserveringvm.SelectedHour = reserveringvm.Reservering.StartTijd.Value.Hour;
                return View(reserveringvm);
            }

            return HttpNotFound();
        }

        // POST: Reserverings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ViewModels.ReserveringViewModel reservering)
        {
            if (ModelState.IsValid)
            {
                Reservering changedReservering = ChangedReservering(reservering);
                if (changedReservering != null)
                {
                    db.Entry(changedReservering).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(reservering);
        }


        public Reservering ChangedReservering(ViewModels.ReserveringViewModel res)
        {
            Reservering changedRes = db.Reserverings.Find(res.ReserveringID);
            if (changedRes != null)
            {
                DateTime newDate = res.SelectedDate;
                changedRes.Datum = newDate;
                changedRes.StartTijd = newDate.AddHours(res.SelectedHour);
                changedRes.EindTijd = changedRes.StartTijd.Value.AddMinutes(120);
                return changedRes;
            }
            return null;
        }
        // GET: Reserverings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservering reservering = db.Reserverings.Find(id);
            if (reservering == null)
            {
                return HttpNotFound();
            }
            return View(reservering);
        }

        public ActionResult ShowKlanten()
        {

            var klanten = db.Users.ToList();
            return View("Klanten", klanten);
        }

        public ActionResult ShowInfo()
        {
            ShowInfoViewModel model = new ShowInfoViewModel();
            var couverts = db.Couverts.ToList();
            return View("Info", couverts);
        }

        //Hier had ik wel moeite mee
        //De code kan waarschijnlijk korter, schooner en duidelijker zijn
        //maar, het werkt.
        //Hier willen we de totaal aantal gebruikte ingredienten voor de datum/tijd gekozen
        [HttpGet]
        public ActionResult GetInfoForDate(int? id)
        {
            Couverts couvert = db.Couverts.Find(id);
            List<Couverts> couverts = db.Couverts.Where(c => c.Reservering.StartTijd.Value == couvert.Reservering.StartTijd.Value).ToList();
            List<TotalIngredients> totalingredients = db.TotalIngredients.ToList();

            int aantal = 0;
            string name = "";
            Dictionary<string, int> ingredientWithAmount = new Dictionary<string, int>();

            //Haal het aantal van de couvert.
            //Dit toont de hoveelheid van een bepaalde menu besteld is
            //we gebruiken dit aantal om de hoeveelheid ingredienten te vermenigvuldigen per bestelt menu
            foreach (Couverts cv in couverts)
            {
                aantal = cv.Aantal;
                foreach (Menu menu in cv.Reservering.BesteldeMenus)
                {
                    foreach (Gerecht gerecht in menu.Gerechten)
                    {
                        var allIngredients = totalingredients.Where(i => i.Gerecht.Id == gerecht.Id);
                        foreach (var ing in allIngredients)
                        {
                            Ingredient singleIngredient = db.Ingredients.Where(x => x.Id == ing.Ingredient.Id).FirstOrDefault();
                            //hier heb ik ook de eenheid toegevoegd aan de 'key'
                            //dit maakt het wat makkelijker te lezen op de view

                            //als ingredient (Key) al bestaat in de Dictionary...
                            if (ingredientWithAmount.ContainsKey(singleIngredient.Naam + " (" + singleIngredient.Eenheid + ") "))
                            {
                                //voegen we de aantal toe (Value)
                                ingredientWithAmount[singleIngredient.Naam + " (" + singleIngredient.Eenheid + ") "] += (ing.Aantal * aantal);
                                continue;
                            }
                            else
                            {
                                //anders voegen wij de ingredient toe
                                name = singleIngredient.Naam+" (" + singleIngredient.Eenheid + ") ";
                                ingredientWithAmount.Add(name, ing.Aantal * aantal);
                            }
                        }
                    }
                }
            }
           
            return View(ingredientWithAmount);
        }

        //Hier sturen we een viewmodel naar een view toe om informatie voor een factuur te tonen
        public ActionResult ShowFactuur(int? id)
        {
            ReserveringCouvertViewMmodel rcvm = new ViewModels.ReserveringCouvertViewMmodel()
            {
                Reservering = db.Reserverings.Find(id),
                Couverts = db.Couverts.Where(c => c.Reservering.Id == id).ToList(),
            };
            return View("Factuur", rcvm);
        }

        // POST: Reserverings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //Omdat er een verbinding betsaat tussen reservering en couvert
            //moeten we eerst de verbinding verwijderen voordat een reservering
            //verwijderd kan worden.
            Reservering reservering = db.Reserverings.Find(id);
            List<Couverts> couverts = db.Couverts.Where(c => c.Reservering.Id == reservering.Id).ToList();
            foreach (Couverts cv in couverts)
            {
                db.Couverts.Remove(cv);
            }

            db.Reserverings.Remove(reservering);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
