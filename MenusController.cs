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

    public class MenusController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Menus
        public ActionResult Index()
        {
            return View(db.Menus.ToList());
        }

        // GET: Menus/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menus.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // GET: Menus/Create
        [Authorize(Roles = ("Admin, Manager, Kok"))]
        public ActionResult Create()
        {
            Menu menu = new Menu
            {
                Gerechten = db.Gerechts.ToList()
            };
            return View(menu);
        }

        //EditorFor on View sends values to Controller
        //ID of Gerecht and Menu are posted here.
        [Authorize(Roles = ("Admin, Manager, Kok"))]
        [HttpPost]
        public ActionResult GerechtToMenu(GerechtMenuViewModel gmViewModel)
        {
            if (ModelState.IsValid)
            {
                //Id's beginnen bij 1
                if (gmViewModel.Gerecht.Id != 0)
                {
                    Gerecht gerecht = db.Gerechts.Where(g => g.Id == gmViewModel.Gerecht.Id).FirstOrDefault();

                    //Id's beginnen bij 1
                    if (gmViewModel.Menu.Id != 0)
                    {
                        Menu menu = db.Menus.Where(m => m.Id == gmViewModel.Menu.Id).FirstOrDefault();
                        menu.Gerechten.Add(gerecht);
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Index");
        }

        //Het verwijderen van een gerecht van een menu
        public ActionResult RemoveDish(int menuId, int dishId)
        {
            Gerecht gerecht = db.Gerechts.Find(dishId);
            if (gerecht.Id != 0)
            {
                Menu menu = db.Menus.Find(menuId);
                if (menu.Id != 0)
                {
                    menu.Gerechten.Remove(gerecht);
                    db.Entry(menu).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        //vul viewmodel met informatie om een gerecht toe te voegen aan een menu
        [Authorize(Roles = ("Admin, Manager, Kok"))]
        public ActionResult AddGerecht(Menu menu)
        {
            if (menu.Id != 0)
            {
                menu = db.Menus.Where(i => i.Id == menu.Id).FirstOrDefault();

                GerechtMenuViewModel gmViewModel = new GerechtMenuViewModel
                {
                    Menu = menu,
                    GerechtList = db.Gerechts.ToList()
                };

                return View(gmViewModel);
            }
            return RedirectToAction("Index");
        }

        // POST: Menus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = ("Admin, Manager, Kok"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Naam,Omschrijving,Prijs")] Menu menu)
        {
            if (ModelState.IsValid)
            {
                db.Menus.Add(menu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(menu);
        }

        // GET: Menus/Edit/5
        [Authorize(Roles = ("Admin, Manager, Kok"))]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menus.Find(id);
            List<Gerecht> gerechten = db.Gerechts.ToList();
            GerechtMenuViewModel gvm = new GerechtMenuViewModel() { Menu = menu, GerechtList = gerechten };
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(gvm);
        }

        // POST: Menus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = ("Admin, Manager, Kok"))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ViewModels.GerechtMenuViewModel model)
        {
            Menu menu = db.Menus.Find(model.Menu.Id);

            if (ModelState.IsValid)
            {
                db.Entry(menu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(menu);
        }

        // GET: Menus/Delete/5
        [Authorize(Roles = ("Admin, Manager, Kok"))]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menus.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // POST: Menus/Delete/5
        [Authorize(Roles = ("Admin, Manager, Kok"))]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //Omdat er een verbinding tussen Menu en Couvert is
        //moeten we eerst de verbinding verwijderen voordat
        //een menu verwijderd kan worden.
        public ActionResult DeleteConfirmed(int id)
        {
            Menu menu = db.Menus.Find(id);
            var couv = db.Couverts.Where(c => c.Menu.Id == menu.Id).ToList();
            foreach (var cv in couv)
            {
                db.Couverts.Remove(cv);
            }
            db.Menus.Remove(menu);
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
