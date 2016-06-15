using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListWebApp.Models;

namespace TodoListWebApp.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private TodoListWebAppContext _db;

        public TodoController(TodoListWebAppContext context)
        {
            _db = context;
        }

        // GET: /Todo/
        public ActionResult Index()
        {
            string owner = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUserToDos = _db.Todoes.Where(a => a.Owner == owner);
            return View(currentUserToDos.ToList());
        }

        // GET: /Todo/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }
            Todo todo = _db.Todoes.Where(t => t.ID == id).FirstOrDefault();
            string owner = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (todo == null || (todo.Owner != owner))
            {
                return new NotFoundResult();
            }
            return View(todo);
        }

        // GET: /Todo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Todo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("ID", "Description")] Todo todo)
        {
            if (ModelState.IsValid)
            {
                todo.Owner = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                _db.Todoes.Add(todo);
                _db.SaveChanges();
                return new RedirectToActionResult("Index", "Todo", null);
            }

            return View(todo);
        }
        // GET: /Todo/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }

            Todo todo = _db.Todoes.Where(t => t.ID == id).FirstOrDefault();
            string owner = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (todo == null || (todo.Owner != owner))
            {
                return new NotFoundResult();
            }
            return View(todo);
        }

        // POST: /Todo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("ID", "Description", "Owner")] Todo todo)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(todo).State = EntityState.Modified;
                _db.SaveChanges();
                return new RedirectToActionResult("Index", "Todo", null);
            }
            return View(todo);
        }

        // GET: /Todo/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }
            Todo todo = _db.Todoes.Where(t => t.ID == id).FirstOrDefault();
            string owner = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (todo == null || (todo.Owner != owner))
            {
                return new NotFoundResult();
            }
            return View(todo);
        }

        // POST: /Todo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Todo todo = _db.Todoes.Where(t => t.ID == id).FirstOrDefault();
            string owner = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (todo == null || (todo.Owner != owner))
            {
                return new BadRequestResult();
            }
            _db.Todoes.Remove(todo);
            _db.SaveChanges();
            return new RedirectToActionResult("Index", "Todo", null);
        }
    }
}
