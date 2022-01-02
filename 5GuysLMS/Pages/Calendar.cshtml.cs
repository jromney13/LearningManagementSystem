using System;
using System.Collections.Generic;
using System.Linq;
using _5GuysLMS.Data;
using _5GuysLMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace _5GuysLMS.Pages
{
    public class CalendarModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;

        public CalendarModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }

        public IList<Course> courses { get; set; }
        public IList<Enrollment> enrollments { get; set; }
        public List<Assignment> assignments { get; set; }
        public User user { get; set; }

        public void OnGet()
        {
            int? id = HttpContext.Session.GetInt32("UserID");
            user = _context.Users.FirstOrDefault(i => i.Id == id);
            assignments = new List<Assignment>();

            if (user.UserType == "Instructor")
            {
                courses = _context.Courses.Include(c => c.Instructor).Where(i => i.Instructor.UserId == id).ToList();

                foreach (var co in courses)
                {
                    IList<Assignment> assign = _context.Assignments.Where(c => c.Course == co).ToList();
                    assignments.AddRange(assign);
                }
            }
            else
            {
                enrollments = _context.Enrollments.Include(u => u.User).Include(c => c.Course).Where(i => i.User.Id == id).ToList();

                foreach (var en in enrollments)
                {
                    IList<Assignment> assign = _context.Assignments.Where(c => c.Course == en.Course).ToList();
                    assignments.AddRange(assign);
                }
            }

        }

        // This method handles when the user clicks the red X on one of the notifications
        public void OnGetRedX(int notificationID, string type)
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
            OnGet(); // Synchronously call the OnGet method
        }
    }
}
