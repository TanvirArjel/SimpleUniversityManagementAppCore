using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManagementAppCore.Data;
using UniversityManagementAppCore.Models;
using UniversityManagementAppCore.ViewModels;

namespace UniversityManagementAppCore.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly UniversityContext _context;

        public InstructorsController(UniversityContext context)
        {
            _context = context;    
        }

        // GET: Instructors
        public async Task<IActionResult> Index(int? id, int? courseId)
        {
            InstructorIndexViewModel instructorIndexViewModel = new InstructorIndexViewModel();
            instructorIndexViewModel.Instructors = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                .ThenInclude(i => i.Course)
                .ThenInclude(i => i.Enrollments)
                .ThenInclude(i => i.Student)
                .Include(i => i.CourseAssignments)
                .ThenInclude(i => i.Course)
                .ThenInclude(i => i.Department)
                .AsNoTracking()
                .OrderBy(i => i.LastName)
                .ToListAsync();

            if (id !=null)
            {
                ViewBag.InstructorId = id;
                instructorIndexViewModel.Courses = instructorIndexViewModel.Instructors
                    .Single(i => i.InstructorId == id).CourseAssignments.Select(ca => ca.Course);
            }

            if (courseId != null)
            {
                ViewBag.CourseId = courseId;
                instructorIndexViewModel.Enrollments = instructorIndexViewModel.Courses
                    .Single(c => c.CourseId == courseId).Enrollments;
            }
            return View(instructorIndexViewModel);
        }

        // GET: Instructors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors.Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments).ThenInclude(ca => ca.Course).AsNoTracking()
                .SingleOrDefaultAsync(m => m.InstructorId == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // GET: Instructors/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.AllCourses = await _context.Courses.ToListAsync();
            return View();
        }

        // POST: Instructors/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,HireDate,OfficeAssignment")] Instructor instructor, int[] selectedCourses)
        {
            if (ModelState.IsValid)
            {
                if (selectedCourses != null)
                {
                    instructor.CourseAssignments = new HashSet<CourseAssignment>();
                    foreach (var selectedCourse in selectedCourses)
                    {
                        CourseAssignment courseAssignment = new CourseAssignment()
                        {
                            InstructorId = instructor.InstructorId,
                            CourseId = selectedCourse
                        };

                        instructor.CourseAssignments.Add(courseAssignment);
                    }
                }
                _context.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.AllCourses = await _context.Courses.ToListAsync();
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors.Include(i => i.OfficeAssignment).Include(i => i.CourseAssignments).ThenInclude(i => i.Course).AsNoTracking().SingleOrDefaultAsync(m => m.InstructorId == id);

            if (instructor == null)
            {
                return NotFound();
            }

            ViewBag.AllCourses = await _context.Courses.ToListAsync();
            return View(instructor);
        }

        private void UpdateInstructorCourses(int[] selectedCourses, Instructor instructorToBeUpdated)
        {
            if (selectedCourses == null)
            {
                instructorToBeUpdated.CourseAssignments = new List<CourseAssignment>();
                return;
            }

            var selectedCoursesHs = new HashSet<int>(selectedCourses);
            var instructorCoursesHs = new HashSet<int>(instructorToBeUpdated.CourseAssignments.Select(c => c.Course.CourseId));

            foreach (var selectedCourse in selectedCoursesHs)
            {
                if (!instructorCoursesHs.Contains(selectedCourse))
                {
                    instructorToBeUpdated.CourseAssignments.Add(new CourseAssignment { InstructorId = instructorToBeUpdated.InstructorId, CourseId = selectedCourse });
                }
            }

            foreach (var instructorCourse in instructorCoursesHs)
            {
                if (!selectedCoursesHs.Contains(instructorCourse))
                {
                    CourseAssignment courseToRemove = instructorToBeUpdated.CourseAssignments.SingleOrDefault(i => i.CourseId == instructorCourse);
                    _context.Remove(courseToRemove);
                }
            }



            //This is my another implementation

            //instructorToBeUpdated.CourseAssignments.Clear();
            //instructorToBeUpdated.CourseAssignments = new List<CourseAssignment>();

            //if (selectedCourses != null)
            //{
                
            //    foreach (var selectedCourse in selectedCourses)
            //    {
            //        CourseAssignment courseAssignment = new CourseAssignment()
            //        {
            //            InstructorId = instructorToBeUpdated.InstructorId,
            //            CourseId = selectedCourse
            //        };

            //        instructorToBeUpdated.CourseAssignments.Add(courseAssignment);
            //    }
            //}


        }

        // POST: Instructors/Edit/5

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id, int[] selectedCourses)
        {
            if (id == null)
            {
                return NotFound();
            }

            Instructor instructorToBeUpdated = await _context.Instructors.Include(i => i.OfficeAssignment).Include(i => i.CourseAssignments).ThenInclude(i => i.Course).SingleOrDefaultAsync(i => i.InstructorId == id);
            if (instructorToBeUpdated == null)
            {
                return NotFound();
            }

            if (await  TryUpdateModelAsync<Instructor>(instructorToBeUpdated,"",i => i.FirstName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))
            {
                if (String.IsNullOrWhiteSpace(instructorToBeUpdated.OfficeAssignment.Location))
                {
                    instructorToBeUpdated.OfficeAssignment = null;
                }

                UpdateInstructorCourses(selectedCourses, instructorToBeUpdated);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ex);
                    ModelState.AddModelError("", "Unable to save changes. Please try again. If the problem persists please contact the system administration");
                }

                return RedirectToAction("Index");
            }

            return View(instructorToBeUpdated);
        }

        // GET: Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors.Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                .ThenInclude(ca => ca.Course).AsNoTracking()
                .SingleOrDefaultAsync(m => m.InstructorId == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await _context.Instructors.Include(i => i.CourseAssignments).SingleOrDefaultAsync(m => m.InstructorId == id);

            var departments = await _context.Departments.Where(d => d.InstructorId == instructor.InstructorId).ToListAsync();
            departments.ForEach(d => d.InstructorId = null);
            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructors.Any(e => e.InstructorId == id);
        }
    }
}
