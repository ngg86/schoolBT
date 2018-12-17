using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BonTemps.Models;
using static BonTemps.Models.ViewModels;

namespace BonTemps.Controllers
{
    [Authorize(Roles = "Admin, Kok, Medewerker")]
    public class GerechtsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Gerechts
        public ActionResult Index()
        {
            //Hier vullen we een viewmodel met gerechten en de hoeveelheid ingredienten die horen bij een gerecht.
            TotalIngredientsGerechtViewModel tigVM = new TotalIngredientsGerechtViewModel
            {
                Gerechts = db.Gerechts.ToList(),
                TotalIngredients = db.TotalIngredients.ToList()
            };
            return View(tigVM);
        }

        // GET: Gerechts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Gerecht gerecht = db.Gerechts.Find(id);
            if (gerecht == null)
            {
                return HttpNotFound();
            }
            return View(gerecht);
        }

        // GET: Gerechts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Gerechts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Naam,Soort")] Gerecht gerecht)
        {
            if (ModelState.IsValid)
            {
                db.Gerechts.Add(gerecht);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(gerecht);
        }

        public ActionResult AddIngredient(int? id)
        {
            //database index begint op 1
            if (id != 0)
            {
                //zoek gerecht die de doorgegeven id heeft
                Gerecht gerecht = db.Gerechts.Find(id);
                if (gerecht != null)
                {
                    //vul viewmodel met de gevonden gerecht en alle ingridienten die in de database zijn
                    ViewModels.IngredientGerechtViewModel igViewModel = new ViewModels.IngredientGerechtViewModel()
                    {
                        GerechtId = (int)id,
                        Gerecht = gerecht,
                        Ingredients = db.Ingredients.ToList()
                    };
                    return View(igViewModel);
                }
            }
            return RedirectToAction("Index");
        }


        //Hier voegen we een ingredient aan een gerecht toe
        [HttpPost]
        public ActionResult IngredientToGerecht(ViewModels.IngredientGerechtViewModel model)
        {
            if (ModelState.IsValid)
            {
                //hier zoeken we de benodigde informatie

                //Het gerecht waar we een ingriedient aan willen toevoegen
                Gerecht gerecht = db.Gerechts.Find(model.GerechtId);
                //Het ingredient dat we willen toevoegen aan het gerecht
                Ingredient ingredient = db.Ingredients.Find(model.Ingredient.Id);

                //Hier zoeken we een mogelijke bestaande verbinding tussen de binnengebrachten gerecht/ingredient
                List<TotalIngredients> totalIngredients = db.TotalIngredients.Where(ti => ti.Gerecht.Id == gerecht.Id).ToList();
                var ing = totalIngredients.Where(i => i.Ingredient.Id == ingredient.Id).FirstOrDefault();
                if (ing == null)
                //Ingredient-Gerecht verbinding bestaat niet
                //dus we maken het verbinding aan
                {
                    TotalIngredients newIngredient = new TotalIngredients()
                    {
                        Aantal = model.Total,
                        Gerecht = gerecht,
                        Ingredient = ingredient
                    };
                    db.TotalIngredients.Add(newIngredient);
                }
                else
                //Als het verbinding tussen gerecht en ingredient wel bestaat, 
                //passen we alleen de aantal van het ingredient aan
                //met de binnengebrachte 'aantal' value.
                {
                    TotalIngredients existingIngredient = totalIngredients.Where(i => i.Ingredient.Id == ingredient.Id).FirstOrDefault();
                    existingIngredient.Aantal = model.Total;
                    db.Entry(existingIngredient).State = EntityState.Modified;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        // GET: Gerechts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Gerecht gerecht = db.Gerechts.Find(id);
            if (gerecht == null)
            {
                return HttpNotFound();
            }
            return View(gerecht);
        }

        // POST: Gerechts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Naam,Soort")] Gerecht gerecht)
        {
            if (ModelState.IsValid)
            {
                db.Entry(gerecht).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(gerecht);
        }

        // GET: Gerechts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Gerecht gerecht = db.Gerechts.Find(id);
            if (gerecht == null)
            {
                return HttpNotFound();
            }
            return View(gerecht);
        }

        // POST: Gerechts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //Doordat er een foreignkey constraint bestaat tussen 
            //gerecht en TotalIngredients, moeten we eerst de verbinding
            //verwijderen voordat we een gerecht definitief verwijdert
            Gerecht gerecht = db.Gerechts.Find(id);
            var ingredients = db.TotalIngredients.Where(i => i.Gerecht.Id == gerecht.Id).ToList();
            foreach (var ing in ingredients)
            {
                db.TotalIngredients.Remove(ing);
            }
            db.Gerechts.Remove(gerecht);
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
