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
    public enum SubmissionTypes { File, Text }

    public class CourseDashboardModel : PageModel
    {
        private readonly _5GuysLMSContext _context;
        public IList<Assignment> Assignments { get; set; }
        [BindProperty] public Course currentCourse { get; set; }
        [BindProperty] public Assignment AddedAssignment { get; set; }

        public CourseDashboardModel(_5GuysLMSContext context)
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
            //check if instructor is for this course
            if (HttpContext.Session.GetInt32("UserID") != currentCourse.Instructor.UserId)
            {

                return NotFound();
            }
            Assignments = await _context.Assignments.Where(a => a.Course == currentCourse).ToListAsync();

            return Page();
        }

        public void AddAssignment(Assignment assignment)
        {
            //Adds new course to database
            _context.Assignments.Add(assignment);
            _context.SaveChanges();
        }

        public async Task<IActionResult> OnPost(int courseID)
        {

            currentCourse = await _context.Courses.
              Include(c => c.Instructor).
              Where(c => c.Id == courseID).
              FirstOrDefaultAsync();

            // Sets the Assignment object's course to currentCourse
            AddedAssignment.Course = currentCourse;

            AddAssignment(AddedAssignment);

            // Grab a list of all students in the current course from the DB
            List<Enrollment> currentCourseEnrollments = await _context.Enrollments.Include(e => e.User).Where(e => e.Course == currentCourse).ToListAsync();
            
            // Create a notification for the newly created assignment for every student enrolled in the current course
            foreach (Enrollment en in currentCourseEnrollments)
            {
                CreatedNotification notificationForStudent = new CreatedNotification();
                notificationForStudent.Assignment = AddedAssignment;
                notificationForStudent.User = en.User;
                notificationForStudent.IsSeen = false; // The notification starts off not having been seen by anyone
                _context.CreatedNotifications.Add(notificationForStudent);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/CourseDashboard", new { courseID = currentCourse.Id });
        }
        public async Task<IActionResult> OnPostDelete(int? Id, int courseId)
        {

            if (Id == null)
            {
                return NotFound();
            }

            Assignment DeleteAssignment = await _context.Assignments.FindAsync(Id);

            if (DeleteAssignment != null)
            {
                _context.Assignments.Remove(DeleteAssignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/CourseDashboard", new { courseId = courseId });

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