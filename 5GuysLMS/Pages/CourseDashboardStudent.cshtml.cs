using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _5GuysLMS.Data;
using _5GuysLMS.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;

namespace _5GuysLMS.Pages
{
    public class CourseDashboardStudentModel : PageModel
    {
        private readonly _5GuysLMSContext _context;
        public IList<Assignment> Assignments { get; set; }
        [BindProperty] public Course currentCourse { get; set; }
        public IList<FileSubmission> fileSubmissions { get; set; }
        public IList<TextSubmission> textSubmissions { get; set; }
        public IList<Enrollment> enrollments { get; set; }
        public double grade { get; set; }

        
        [BindProperty] public String letterGrade { get; set; }
        [BindProperty] public String gradePercentage { get; set; }

        public List<double> studentPercentages = new List<double>();

        public CourseDashboardStudentModel(_5GuysLMSContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync(int courseID)
        {
            currentCourse = await _context.Courses.
                Include(c => c.Instructor).
                Where(c => c.Id == courseID).
                FirstOrDefaultAsync();

            if (currentCourse == default)
            {
                //course wasn't found
                return NotFound();
            }
          
            Assignments = await _context.Assignments.Where(a => a.Course == currentCourse).ToListAsync();

            //
            int? UserId = HttpContext.Session.GetInt32("UserID");
            double percent = CalcGradePercentage(currentCourse, (int)UserId);
            grade = percent * 100;
            gradePercentage = percent.ToString("P", CultureInfo.InvariantCulture);

            if (percent >= 0.94)
            {
                letterGrade = "A";
            }
            else if (percent >= 0.9)
            {
                letterGrade = "A-";
            }
            else if (percent >= 0.87)
            {
                letterGrade = "B+";
            }
            else if (percent >= 0.84)
            {
                letterGrade = "B";
            }
            else if (percent >= 0.8)
            {
                letterGrade = "B-";
            }
            else if (percent >= 0.77)
            {
                letterGrade = "C+";
            }
            else if (percent >= 0.74)
            {
                letterGrade = "C";
            }
            else if (percent >= 0.7)
            {
                letterGrade = "C-";
            }
            else if (percent >= 0.67)
            {
                letterGrade = "D+";
            }
            else if (percent >= 0.64)
            {
                letterGrade = "D";
            }
            else if (percent >= 0.6)
            {
                letterGrade = "D-";
            }
            else
            {
                letterGrade = "F";
            }

            enrollments = await _context.Enrollments.Include(u => u.User).Where(e => e.Course.Id == currentCourse.Id).ToListAsync();

            foreach (var enroll in enrollments)
            {
                double percentToAdd = CalcGradePercentage(currentCourse, enroll.User.Id);
                studentPercentages.Add(percentToAdd * 100);
            }

            return Page();
        }

        // This method handles when the user clicks the red X on one of the notifications
        public async Task<IActionResult> OnGetRedX(int courseID, int notificationID, string type)
        {
            if (type == "created")
            {
                List<Models.CreatedNotification> cns = HttpContext.Session.GetComplexData<List<Models.CreatedNotification>>("CreatedNotifications");
                Models.CreatedNotification queriedCreatedNotification = cns.SingleOrDefault(cn => cn.Id == notificationID);
                if (queriedCreatedNotification != default) cns.Remove(queriedCreatedNotification);
                HttpContext.Session.SetComplexData("CreatedNotifications", cns);
            }
            else if (type == "graded")
            {
                List<Models.GradedNotification> gns = HttpContext.Session.GetComplexData<List<Models.GradedNotification>>("GradedNotifications");
                Models.GradedNotification queriedGradedNotification = gns.SingleOrDefault(gn => gn.Id == notificationID);
                if (queriedGradedNotification != default) gns.Remove(queriedGradedNotification);
                HttpContext.Session.SetComplexData("GradedNotifications", gns);
            }
            return await OnGetAsync(courseID); // Asynchronously return the OnGetAsync method
        }

        private double CalcGradePercentage(Course course, int UserId, int max = 0, int grade = 0)
        {
            textSubmissions = _context.TextSubmissions.
              Include(a => a.User).Include(a => a.Assignment).Where(a => a.Assignment.Course.Id == course.Id).Where(u => u.User.Id == UserId).ToList();

            fileSubmissions = _context.FileSubmissions.
               Include(a => a.User).Include(a => a.Assignment).Where(a => a.Assignment.Course.Id == course.Id).Where(u => u.User.Id == UserId).ToList();

            if(fileSubmissions.Count() == 0 && textSubmissions.Count() == 0)
            {
                return 0;
            }

            foreach (var textSub in textSubmissions)
            {
                if (textSub.IsGraded == true)
                {
                    grade += textSub.PointsReceived;
                    max += textSub.Assignment.MaxPoints;
                }
            }
            foreach (var fileSub in fileSubmissions)
            {
                if (fileSub.IsGraded == true)
                {
                    grade += fileSub.PointsReceived;
                    max += fileSub.Assignment.MaxPoints;
                }
            }

            if(grade == 0 && max == 0)
            {
                return 0;
            }

            return ((double)(grade) / max);
        }
    }
}
