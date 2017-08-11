using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UniversityManagementAppCore.Models;

namespace UniversityManagementAppCore.ViewModels
{
    public class EnrollmentCountByDate
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0: dd-MMM-yyyy}")]
        public DateTime?  EnrollmentDate { get; set; }
        public int StudentCount { get; set; }
    }

    public class InstructorIndexViewModel
    {
        public IEnumerable<Instructor> Instructors { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<Enrollment> Enrollments { get; set; }
    }
}
