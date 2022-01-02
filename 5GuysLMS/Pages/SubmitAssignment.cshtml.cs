using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using _5GuysLMS.Data;
using _5GuysLMS.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace _5GuysLMS.Pages
{
    public class SubmitAssignmentModel : PageModel
    {
        private readonly _5GuysLMSContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        [BindProperty] public Assignment currentAssignment { get; set; }
        [BindProperty] public User currentUser { get; set; }
        [BindProperty] public TextSubmission currentTextSubmission { get; set; }
        [BindProperty] public FileSubmission currentFileSubmission { get; set; }
        [BindProperty] public IFormFile File { set; get; }

        public SubmitAssignmentModel(_5GuysLMSContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _hostingEnvironment = environment;
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

            return Page();
        }

        public async Task<IActionResult> OnPost(int courseID, int assignmentID)
        {
            currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == HttpContext.Session.GetInt32("UserID"));

            currentAssignment = await _context.Assignments.
            Include(a => a.Course).
            Where(a => a.Id == assignmentID && a.Course.Id == courseID).
            FirstOrDefaultAsync();

            // This is for text type submissions
            if (currentAssignment.SubmissionType == "Text")
            {
                // Set the values of the currentTextSubmission
                currentTextSubmission.IsLate = DateTime.Now > currentAssignment.DueDate;
                currentTextSubmission.User = currentUser;
                currentTextSubmission.Assignment = currentAssignment;
                currentTextSubmission.IsGraded = false; // An assignment doesn't start out graded

                // Adds the new TextSubmission to database
                _context.TextSubmissions.Add(currentTextSubmission);

                await _context.SaveChangesAsync();
                return RedirectToPage("/CourseDashboardStudent", new { courseID = currentAssignment.Course.Id });
            }
            // This is for file type submissions
            else
            {
                // logic for file submissions

                if (File != null)
                {
                    currentFileSubmission.fileName = File.FileName;  // set the original file name
                    var fileName = GetUniqueFileName(File.FileName);
                    var assignmentUploads = Path.Combine(_hostingEnvironment.WebRootPath, "assignmentUploads");
                    var filePath = Path.Combine(assignmentUploads, fileName);
                    File.CopyTo(new FileStream(filePath, FileMode.Create));
                    currentFileSubmission.savedName = fileName; // Set the saved file name

                    currentFileSubmission.IsLate = DateTime.Now > currentAssignment.DueDate;
                    currentFileSubmission.User = currentUser;
                    currentFileSubmission.Assignment = currentAssignment;
                    currentFileSubmission.IsGraded = false; // An assignment doesn't start out graded

                    _context.FileSubmissions.Add(currentFileSubmission); // add submission to DB
                    await _context.SaveChangesAsync();

                    return RedirectToPage("/CourseDashboardStudent", new { courseID = currentAssignment.Course.Id });
                }

                return NotFound();
            }
        }
        
        /// <summary>
        /// Appends the filename with a GUID to avoid duplicate file name submissions
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetUniqueFileName(string name)
        {
            string fileName = Path.GetFileName(name);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_" + Guid.NewGuid().ToString().Substring(0, 15)
                   + Path.GetExtension(fileName);
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
