using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManagementAppCore.Data;
using UniversityManagementAppCore.Models;

namespace UniversityManagementAppCore.Controllers
{
    public class CoursesController : Controller
    {
        private readonly UniversityContext _context;

        public CoursesController(UniversityContext context)
        {
            _context = context;    
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var courses = _context.Courses.Include(c => c.Department).AsNoTracking();
            return View(await courses.ToListAsync());
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Department)
                .SingleOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }


        public void PopulateDepartmentDropDownList(object selectedDepartment = null)
        {
            var departments = _context.Departments.AsNoTracking().ToList();
            ViewBag.DepartmentList = new SelectList(departments, "DepartmentId", "Name", selectedDepartment);
        }
        // GET: Courses/Create
        public IActionResult Create()
        {
            PopulateDepartmentDropDownList();
            return View();
        }

        // POST: Courses/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,Title,Credits,DepartmentId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            PopulateDepartmentDropDownList(course.DepartmentId);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.AsNoTracking().SingleOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }
            PopulateDepartmentDropDownList(course.DepartmentId);
            return View(course);
        }

        // POST: Courses/Edit/5
        
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Course courseToBeUpdated = await _context.Courses.SingleOrDefaultAsync(x => x.CourseId == id);

            if (courseToBeUpdated == null)
            {
                return NotFound();
            }

            if (await  TryUpdateModelAsync<Course>(courseToBeUpdated,"",c => c.Title,c => c.DepartmentId, c=> c.Credits))
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ex);
                    ModelState.AddModelError("","Unable to save changes. Please try again. If the problem persists please contact the system administration");
                }
                return RedirectToAction("Index");
            }
            
            PopulateDepartmentDropDownList(courseToBeUpdated.DepartmentId);
            return View(courseToBeUpdated);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Department).AsNoTracking()
                .SingleOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.SingleOrDefaultAsync(m => m.CourseId == id);
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
