using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _5GuysLMS.Data;
using _5GuysLMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace _5GuysLMS.Pages
{
    public class EditAssignmentModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;
        [BindProperty] public Assignment assignment { get; set; }
        [BindProperty] public int courseid { get; set; }

        public EditAssignmentModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync(int id, int courseId)
        {
            assignment = await _context.Assignments.FirstOrDefaultAsync(c => c.Id == id);

            courseid = courseId;
            if (assignment == default)
            {
                //course wasn't found
                return NotFound();
            }


            return Page();
        }

        public async Task<IActionResult> OnPost(int? courseId)
        {

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(assignment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssignmentExists(assignment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/CourseDashboard", new { courseId = courseId });
            
            // await _context.SaveChangesAsync();

        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(a => a.Id == id);
        }

        // This method handles when the user clicks the red X on one of the notifications
        public async Task<IActionResult> OnGetRedX(int id, int courseId, int notificationID, string type)
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
            return await OnGetAsync(id, courseId); // Asynchronously return the OnGetAsync method
        }
    }
}
