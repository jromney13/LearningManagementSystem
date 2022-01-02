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
    public class LogoutModel : PageModel
    {
        private readonly _5GuysLMSContext _context;

        public LogoutModel(_5GuysLMSContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            // Get lists of the CreatedNotifications and GradedNotifications
            List<Models.CreatedNotification> cns = await _context.CreatedNotifications.Include(cn => cn.User).ToListAsync();
            List<Models.GradedNotification> gns = await _context.GradedNotifications.Include(gn => gn.FileSubmission).Include(gn => gn.TextSubmission).Include(gn => gn.FileSubmission.User).Include(gn => gn.TextSubmission.User).ToListAsync();
            foreach (Models.CreatedNotification cn in cns)
            {
                if (cn.User.Id == HttpContext.Session.GetInt32("UserID"))
                {
                    // Only mark the notification as seen if it is this user's
                    cn.IsSeen = true;
                }
            }
            foreach (Models.GradedNotification gn in gns)
            {
                if (gn.IsFileSubmissionNotification && gn.FileSubmission.User.Id == HttpContext.Session.GetInt32("UserID"))
                {
                    // Only mark the notification as seen if it is this user's
                    gn.IsSeen = true;
                }
                else if (gn.TextSubmission.User.Id == HttpContext.Session.GetInt32("UserID"))
                {
                    // Only mark the notification as seen if it is this user's
                    gn.IsSeen = true;
                }
            }

            // Logging out clears all old notifications, so delete them from the DB
            _context.CreatedNotifications.RemoveRange(cns.Where(cn => cn.IsSeen));
            _context.GradedNotifications.RemoveRange(gns.Where(gn => gn.IsSeen));

            await _context.SaveChangesAsync(); // Save changes to the DB

            HttpContext.Session.Clear();
            return RedirectToPage("Index");
        }
    }
}
