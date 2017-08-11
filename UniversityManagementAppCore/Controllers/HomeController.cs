using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManagementAppCore.Data;
using UniversityManagementAppCore.ViewModels;

namespace UniversityManagementAppCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly UniversityContext _context;

        public HomeController(UniversityContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            IQueryable<EnrollmentCountByDate> enrollmentCountByDates = _context.Students.GroupBy(s => s.EnrollmentDate).Select(group => new EnrollmentCountByDate
            {
                EnrollmentDate = @group.Key,
                StudentCount = @group.Count()
            });


            return View(await enrollmentCountByDates.AsNoTracking().ToListAsync());
        }

       
        public IActionResult Error()
        {
            return View();
        }
    }
}
