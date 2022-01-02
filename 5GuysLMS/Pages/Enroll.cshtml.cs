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
    public class EnrollModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;

        public EnrollModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }

        public IList<Course> Course { get; set; }
        public IList<Enrollment> EnrolledCourses { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchInput { get; set; }

        [BindProperty(SupportsGet = true)]
        public Departments? DropDownInput { get; set; }


        public async Task OnGetAsync()
        {   //get all courses 
            Course = await _context.Courses.Include(c => c.Instructor.User).ToListAsync();
            //get courses enrolled in already
            EnrolledCourses = await _context.Enrollments.
                Include(e => e.User).
                Include(e => e.Course).
                Where(e => e.User.Id == HttpContext.Session.GetInt32("UserID")).
                ToListAsync();

            //Search Function
            var courses = from c in _context.Courses
                          select c;
            if (!string.IsNullOrEmpty(SearchInput))
            {
                courses = courses.Where(c => c.CourseTitle.Contains(SearchInput));
            }
            if (!string.IsNullOrEmpty(DropDownInput.ToString()))
            {
                courses = courses.Where(c => c.CourseDepartment.Contains(DropDownInput.ToString()));
            }

            Course = await courses.ToListAsync();
        }

        public async Task<IActionResult> OnPostEnrollAsync(int CourseID){
            // add enrollment for id
            Enrollment enrollment = new Enrollment();
            enrollment.Course = await _context.Courses.FindAsync(CourseID);
            enrollment.User = await _context.Users.FindAsync(HttpContext.Session.GetInt32("UserID"));
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();
            await OnGetAsync();
            SetSessionCourses();
            return Page();

        }

        public async Task<IActionResult> OnPostDropAsync(int CourseID) {
            // remove enrollment for ID
            Enrollment toDrop = _context.Enrollments.
                Include(e=>e.User).
                Where(e=>e.Course.Id == CourseID && e.User.Id == HttpContext.Session.GetInt32("UserID")).
                FirstOrDefault();
            _context.Enrollments.Remove(toDrop);
            await _context.SaveChangesAsync();
            await OnGetAsync();
            SetSessionCourses();
            return Page();        
        }

        public async void SetSessionCourses()
        {
            User queriedUser = await _context.Users.FindAsync(HttpContext.Session.GetInt32("UserID"));
            if (queriedUser.UserType == "Student")
            {
                // Get courses the user is enrolled in. Stores them in a list
                List<Enrollment> EnrolledCourses = await _context.Enrollments.
                      Include(e => e.User).
                      Include(e => e.Course).
                      Where(e => e.User.Id == HttpContext.Session.GetInt32("UserID")).
                      ToListAsync();

                //store our courses in the session
                HttpContext.Session.SetComplexData("CourseList", EnrolledCourses);
            }
        }

        // This method handles when the user clicks the red X on one of the notifications
        public async Task OnGetRedX(int notificationID, string type)
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
            await OnGetAsync(); // Asynchronously call the OnGetAsync method
        }
    }
}
