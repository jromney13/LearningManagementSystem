using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using _5GuysLMS.Data;
using _5GuysLMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace _5GuysLMS.Pages
{
    public class EditCourseModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;
        [BindProperty] public Course currentCourse { get; set; }

        public EditCourseModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context; 
        }

        public async Task<IActionResult> OnGetAsync(int courseID)
        {
            currentCourse = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseID);
            

            if (currentCourse == default)
            {
                //course wasn't found
                return NotFound();
            }


            return Page();
        }
        public async Task<IActionResult> OnPost()
        {
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(currentCourse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(currentCourse.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/AddCourse");
           // await _context.SaveChangesAsync();
            
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(c => c.Id == id);
        }

        // This method handles when the user clicks the red X on one of the notifications
        public async Task<IActionResult> OnGetRedX(int courseId, int notificationID, string type)
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
            return await OnGetAsync(courseId); // Asynchronously return the OnGetAsync method
        }
    }
}
