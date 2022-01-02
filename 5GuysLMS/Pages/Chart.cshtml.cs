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

namespace _5GuysLMS.Pages
{
    public class ChartModel : PageModel
    {
        private readonly _5GuysLMSContext _context;
        public IList<Enrollment> enrollments { get; set; }
        public IList<Assignment> assignments { get; set; }
        public IList<FileSubmission> fileSubmissions { get; set; }
        public IList<TextSubmission> textSubmissions { get; set; }

        public List<int> grades = new List<int>();
        public List<int> gradeGroup = new List<int>();
        public List<String> students = new List<String>();
        public List<int> maxList = new List<int>();

        public int numStudents { get; set; }

        [BindProperty] public int grade { get; set; }
        [BindProperty] public int max { get; set; }
        [BindProperty] public Course course { get; set; }

        public ChartModel(_5GuysLMSContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync(int courseID)
        {
            enrollments = await _context.Enrollments.
                Include(e => e.Course).Include(e => e.User).
                Where(e => e.Course.Id == courseID).ToListAsync();
            
            course = await _context.Courses.Where(e => e.Id == courseID).FirstOrDefaultAsync();

            assignments = await _context.Assignments.
                Include(a => a.Course).Where(a => a.Course.Id == courseID).ToListAsync();

            textSubmissions = await _context.TextSubmissions.
               Include(a => a.User).Include(a => a.Assignment).Where(a => a.Assignment.Course.Id == courseID).ToListAsync();

            fileSubmissions = await _context.FileSubmissions.
               Include(a => a.User).Include(a => a.Assignment).Where(a => a.Assignment.Course.Id == courseID).ToListAsync();

            int iter = 0;
            foreach (var enrollment in enrollments)
            {
                foreach(var textSub in textSubmissions)
                {
                    if(textSub.User.Id == enrollment.User.Id && textSub.IsGraded == true)
                    {
                        grade += textSub.PointsReceived;
                        max += textSub.Assignment.MaxPoints;
                    }
                }
                foreach(var fileSub in fileSubmissions)
                {
                    if (fileSub.User.Id == enrollment.User.Id && fileSub.IsGraded == true)
                    {
                        grade += fileSub.PointsReceived;
                        max += fileSub.Assignment.MaxPoints;
                    }
                }
               
                students.Add(enrollment.User.FirstName + " " + enrollment.User.LastName);
                grades.Add(grade);
                grade = 0;
                maxList.Add(max);
                max = 0;
                iter++;
            
            }
            numStudents = iter;
            int a = 0;
            int b = 0;
            int c = 0;
            int d = 0;
            int f = 0;
            int count = 0;
            foreach (var item in grades)
            {
                if(item >= (0.9 * maxList[count]))
                {
                    a++;
                }
                else if(item >= (0.8 * maxList[count]))
                {
                    b++;
                }
                else if (item >= (0.7 * maxList[count]))
                {
                    c++;
                }
                else if (item >= (0.6 * maxList[count]))
                {
                    d++;
                }
                else
                {
                    f++;
                }
                count++;
            }
            gradeGroup.Add(a);
            gradeGroup.Add(b);
            gradeGroup.Add(c);
            gradeGroup.Add(d);
            gradeGroup.Add(f);

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
    }
}
