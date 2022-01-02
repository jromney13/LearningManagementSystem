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
    public class SubmissionsModel : PageModel
    {

        private readonly _5GuysLMSContext _context;
        [BindProperty] public Assignment assignment { get; set; }
        public IList<TextSubmission> textSubmissions { get; set; }
        public IList<FileSubmission> fileSubmissions { get; set; }

        public SubmissionsModel(_5GuysLMSContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync(int id, int courseID)
        {
            assignment = await _context.Assignments.FirstOrDefaultAsync(c => c.Id == id);

            textSubmissions = await _context.TextSubmissions.Include(c => c.User).Where(a => a.Assignment.Id == assignment.Id).ToListAsync();
            fileSubmissions = await _context.FileSubmissions.Include(c => c.User).Where(a => a.Assignment.Id == assignment.Id).ToListAsync();



            if (assignment == default)
            {
                //course wasn't found
                return NotFound();
            }


            return Page();

            //currentCourse = await _context.Courses.
            //    Include(c => c.Instructor).
            //    Where(c => c.Id == courseID).
            //    FirstOrDefaultAsync();

            //if (currentCourse == default)
            //{
            //    //course wasn't found
            //    return NotFound();
            //}
            ////check if instructor is for this course
            //if (HttpContext.Session.GetInt32("UserID") != currentCourse.Instructor.UserId)
            //{

            //    return NotFound();
            //}
            //Assignments = await _context.Assignments.Where(a => a.Course == currentCourse).ToListAsync();

          
        }

        // This method handles when the user clicks the red X on one of the notifications
        public async Task<IActionResult> OnGetRedX(int id, int courseID, int notificationID, string type)
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
            return await OnGetAsync(id, courseID); // Asynchronously return the OnGetAsync method
        }
    }
}
