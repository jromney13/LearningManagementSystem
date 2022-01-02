using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _5GuysLMS.Data;
using _5GuysLMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace _5GuysLMS.Pages
{
    public class CourseDetailsModel : PageModel
    {
        private readonly _5GuysLMSContext _context;

        public IList<FileSubmission> FileSubmissions { get; set; }
        public IList<TextSubmission> TextSubmissions { get; set; }


        public List<int> UserScores = new List<int>();
        public List<int> ClassScores = new List<int>();

        [BindProperty] public Assignment currentAssignment { get; set; }
        [BindProperty] public User currentUser { get; set; }

        public CourseDetailsModel(_5GuysLMSContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int courseID, int assignmentID)
        {
            int? id = HttpContext.Session.GetInt32("UserID");

            if (id == null)
            {
                // User wasn't found
                return NotFound();
            }

            currentAssignment = await _context.Assignments.
                Include(a => a.Course).
                Where(a => a.Id == assignmentID && a.Course.Id == courseID).
                FirstOrDefaultAsync();

            currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (currentAssignment == default)
            {
                // Assignment wasn't found
                return NotFound();
            }

            if (currentAssignment.Course.Id != courseID)
            {
                // The course of the assignment and the passed in courseID don't match
                return NotFound();
            }

            //  File Submission Type
            if(currentAssignment.SubmissionType == "File")
            {
                FileSubmissions = await _context.FileSubmissions.Where(a => a.Assignment == currentAssignment).ToListAsync();

                foreach (var item in FileSubmissions)
                {
                    ClassScores.Add(item.PointsReceived);
                    if (item.User.Id == id)
                    {
                        UserScores.Add(item.PointsReceived);
                    }
                }
                return Page();
            }

            // Text Submission Type
            TextSubmissions = await _context.TextSubmissions.Where(a => a.Assignment == currentAssignment).ToListAsync();

            foreach (var item in TextSubmissions)
            {
                ClassScores.Add(item.PointsReceived);
                if (item.User.Id == id)
                {
                    UserScores.Add(item.PointsReceived);
                }
            }

            return Page();
        }

        // This method handles when the user clicks the red X on one of the notifications
        public async Task<IActionResult> OnGetRedX(int courseID, int assignmentID, int notificationID, string type)
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
            return await OnGetAsync(courseID, assignmentID); // Asynchronously return the OnGetAsync method
        }
    }
}
