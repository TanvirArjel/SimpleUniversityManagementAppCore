using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManagementAppCore.CommonCode;
using UniversityManagementAppCore.Data;
using UniversityManagementAppCore.Models;
using System.Linq.Dynamic.Core;
using System.Net.Mime;
using System.Runtime.CompilerServices;

namespace UniversityManagementAppCore.Controllers
{
    public class StudentsController : Controller
    {
        private readonly UniversityContext _context;

        public StudentsController(UniversityContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index(string firstName, string lastName, string sortOrder = "FirstName_asc", int page = 1)
        {
            var students = _context.Students.AsQueryable();
            if (!String.IsNullOrEmpty(firstName) || !String.IsNullOrEmpty(lastName))
            {
                students = _context.Students.Where(s => s.FirstName.Contains(firstName) || s.LastName.Contains(lastName));
            }

            var sortColumnName = sortOrder.EndsWith("_asc") ? sortOrder.Substring(0, sortOrder.Length - 4) : sortOrder.Substring(0, sortOrder.Length - 5);

            var sortOrderName = sortOrder.Substring(sortOrder.IndexOf("_") + 1);
            students = students.OrderBy(sortColumnName + " " + sortOrderName);


            #region BeginnerProcess
            //bool ascending = sortOrder.EndsWith("_asc");
            //if (ascending)
            //{
            //    students = students.OrderBy(e => EF.Property<object>(e, sortColumnName));
            //}
            //else
            //{
            //    students = students.OrderByDescending(e => EF.Property<object>(e, sortColumnName));
            //}


            //switch (sortOrder)
            //{
            //    case "name_desc":
            //        students = students.OrderByDescending(s => s.FirstName);
            //        break;
            //    case "date_desc":
            //        students = students.OrderByDescending(s => s.EnrollmentDate);
            //        break;
            //    case "date_asc":
            //        students = students.OrderBy(s => s.EnrollmentDate);
            //        break;
            //    default:
            //        students = students.OrderBy(s => s.FirstName);
            //        break;
            //}
            #endregion



            ViewData["currentFirstNameFilter"] = firstName;
            ViewData["currentLastNameFilter"] = lastName;

            ViewData["currentSortOrder"] = sortOrder;

            ViewData["NameSortParam"] = sortOrder == "FirstName_asc" ? "FirstName_desc" : "FirstName_asc";
            ViewData["DateSortParam"] = sortOrder == "EnrollmentDate_desc" ? "EnrollmentDate_asc" : "EnrollmentDate_desc";


            const int pageSize = 2;

            var totalItems = students.Count();
            ViewBag.TotalItem = totalItems;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.PageButtonToShow = 5;
            ViewBag.TotalPage = Convert.ToInt32(Math.Ceiling((double)totalItems / pageSize));

            int pageItemStarts = 0;
            if (totalItems > 0)
            {
                pageItemStarts = ((page - 1) * pageSize) + 1;
            }
            ViewBag.PageItemStarts = pageItemStarts;

            int pageItemTo = 0;
            if (totalItems > 0)
            {
                if (page * pageSize > totalItems)
                {
                    pageItemTo = totalItems;
                }
                else
                {
                    pageItemTo = page * pageSize;
                }
            }

            ViewBag.PageItemTo = pageItemTo;


            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), page, pageSize));
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.Include(s => s.Enrollments).ThenInclude(e => e.Course)
                .SingleOrDefaultAsync(m => m.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.AllCourses = await _context.Courses.ToListAsync();
            return View();

        }

        // POST: Students/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,EnrollmentDate")] Student student, int[] selectedCourses)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (selectedCourses != null)
                    {
                        student.Enrollments = new List<Enrollment>();
                        foreach (var selectedCourse in selectedCourses)
                        {
                            Enrollment enrollment = new Enrollment() { CourseId = selectedCourse, StudentId = student.StudentId, Grade = null };
                            student.Enrollments.Add(enrollment);
                        }
                    }
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError("", "Uable to save changes.Try again, and if the problem persists, see your system administrator.");
            }
            ViewBag.AllCourses = await _context.Courses.ToListAsync();
            return View(student);
        }



        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.Include(s => s.Enrollments).ThenInclude(e => e.Course).SingleOrDefaultAsync(m => m.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }

            ViewBag.AllCourses = await _context.Courses.ToListAsync();
            return View(student);
        }

        private void UpdateStudentEnrollments(Student studentToBeUpdated, int[] selectedCourses, string [] grades)
        {
            if (selectedCourses == null)
            {
                studentToBeUpdated.Enrollments = new List<Enrollment>();
                return;
            }

            var selectedCourseHs = new HashSet<int>(selectedCourses);
            var gradesHs = new HashSet<string>(grades);

            studentToBeUpdated.Enrollments.Clear();

            studentToBeUpdated.Enrollments = new List<Enrollment>();

            using (var e1 = selectedCourseHs.GetEnumerator())
            using (var e2 = gradesHs.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    var selectedCourseId = e1.Current;
                    var currentGrade = e2.Current;
                 
                    Grade grade;
                    Enrollment enrollment;
                    bool tryParse = Enum.TryParse(currentGrade,true, out grade);
                    if (tryParse)
                    {
                        enrollment = new Enrollment()
                        {
                            CourseId = selectedCourseId,
                            Grade = grade,
                            StudentId = studentToBeUpdated.StudentId
                        };
                    }
                    else
                    {
                        enrollment = new Enrollment()
                        {
                            CourseId = selectedCourseId,
                            StudentId = studentToBeUpdated.StudentId
                        };
                    }
                    studentToBeUpdated.Enrollments.Add(enrollment);
                }
            }

        }

        // POST: Students/Edit/5

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id, int[] selectedCourses, string[] grades)
        {
            if (id == null)
            {
                return NotFound();
            }

            Student studentToBeUpdated = await _context.Students.Include(s => s.Enrollments).ThenInclude(e => e.Course).SingleOrDefaultAsync(s => s.StudentId == id);
            if (studentToBeUpdated == null)
            {
                return NotFound();
            }
            if (await TryUpdateModelAsync<Student>(studentToBeUpdated, "", s => s.FirstName, s => s.LastName, s => s.EnrollmentDate))
            {
                try
                {
                    UpdateStudentEnrollments(studentToBeUpdated, selectedCourses, grades);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ex);
                    ModelState.AddModelError("", "Uable to save chages! Plese Try again. If the problem persits, see your system administrator");
                }
            }



            ViewBag.AllCourses = await _context.Courses.ToListAsync();
            return View(studentToBeUpdated);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.Include(s => s.Enrollments).ThenInclude(e => e.Course)
                .SingleOrDefaultAsync(m => m.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studentToBeDeleted = await _context.Students.AsNoTracking().SingleOrDefaultAsync(m => m.StudentId == id);
            if (studentToBeDeleted == null)
            {
                return RedirectToAction("Index");
            }
            try
            {
                _context.Students.Remove(studentToBeDeleted);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex);
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
    }
}
