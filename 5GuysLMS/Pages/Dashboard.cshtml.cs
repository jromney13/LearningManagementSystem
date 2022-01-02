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
    public class DashboardModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;

        public DashboardModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }

        public User User { get; set; }
        public Instructor Instructor { get; set; }
        public IList<Course> Courses { get; set; }
        public IList<Enrollment> EnrolledCourses { get; set; }
        [BindProperty] public List <Assignment> AssignmentsToDo { get; set; }
        [BindProperty(SupportsGet = true)]
        public string assignmentFilter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            int? id = HttpContext.Session.GetInt32("UserID");
            AssignmentsToDo = new List<Assignment>();

            if (id == null)
            {
                return NotFound();
            }

            User = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (User == null)
            {
                return NotFound();
            }
            
            if(User.UserType == "Instructor")
            {
                Instructor = await _context.Instructors.Where(i=>i.UserId == id).FirstOrDefaultAsync();
                // TODO: FIX QUERY FREQUENCY
                Courses = _context.Courses.Include(c=>c.Instructor).Where(t => t.Instructor.Id == Instructor.Id).ToList();

                // Get all of the assignments for courses that the instructor is teaching
                foreach (Course course in Courses)
                {
                    IList<Assignment> courseAssignments = await _context.Assignments.Where(a => a.Course == course).ToListAsync();
                    AssignmentsToDo.AddRange(courseAssignments);
                }
            }
            else
            {

                //get our enrollments from the session instead. 
                EnrolledCourses = HttpContext.Session.GetComplexData<List<Enrollment>>("CourseList");
               
                // Get all of the assignments for courses that the student is in
                foreach (Enrollment enrollment in EnrolledCourses)
                {
                    IList<Assignment> courseAssignments = await _context.Assignments.Include(a => a.Course).Where(a => a.Course == enrollment.Course).ToListAsync();
                    AssignmentsToDo.AddRange(courseAssignments);
                }
            }

            if (assignmentFilter == "Past")
            {
                // Only assignments whose due date is in the past
                AssignmentsToDo = AssignmentsToDo.Where(a => a.DueDate < DateTime.Now).ToList();
            }
            else
            {
                // Only assignments whose due date is in the future
                AssignmentsToDo = AssignmentsToDo.Where(a => a.DueDate > DateTime.Now).ToList();
            }

            // Sort the assignments chronologically
            AssignmentsToDo.Sort((assignment1, assignment2) => DateTime.Compare(assignment1.DueDate, assignment2.DueDate));

            // Only 5 assignments can display in the To-Do list at one time
            if (AssignmentsToDo.Count > 5)
            {
                // Remove all but the first five elements
                AssignmentsToDo.RemoveRange(5, AssignmentsToDo.Count - 5);
            }

            return Page();
        }

        // This method handles when the user clicks the red X on one of the notifications
        public async Task<IActionResult> OnGetRedX(int notificationID, string type)
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
            return await OnGetAsync(); // Asynchronously return the OnGetAsync method
        }
    }
}
