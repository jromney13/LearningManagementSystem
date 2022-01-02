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
using System.IO;
using Microsoft.AspNetCore.Hosting;


namespace _5GuysLMS.Pages
{
    public class GradeAssignmentModel : PageModel
    {
        private readonly _5GuysLMSContext _context;
        [BindProperty] public TextSubmission textSubmission { get; set; }
        [BindProperty] public FileSubmission fileSubmission { get; set; }
        [BindProperty] public Assignment assignment { get; set; }

        [BindProperty] public int submissionId { get; set; }

        private IHostingEnvironment Environment;
       
        public GradeAssignmentModel(_5GuysLMSContext context, IHostingEnvironment _environment)
        {
            _context = context;
            this.Environment = _environment;
        }
        public async Task<IActionResult> OnGetAsync(int id, String subType)
        {
            if(subType == "Text")
            {
                textSubmission = await _context.TextSubmissions.Include(c => c.User).Include(c => c.Assignment).FirstOrDefaultAsync(c => c.Id == id);

                assignment = textSubmission.Assignment;

                submissionId = textSubmission.Id;

                if (textSubmission == default)
                {
                    //Submission wasn't found
                    return NotFound();
                }
            }
            else if(subType == "File")
            {
                fileSubmission = await _context.FileSubmissions.Include(c => c.User).Include(c => c.Assignment).FirstOrDefaultAsync(c => c.Id == id);

                assignment = fileSubmission.Assignment;

                submissionId = fileSubmission.Id;

                if (fileSubmission == default)
                {
                    //Submission wasn't found
                    return NotFound();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(int subId, String subType)
        {
            
            if (subType == "Text")
            {
                TextSubmission submission = await _context.TextSubmissions.FirstOrDefaultAsync(s => s.Id == subId);
                submission.PointsReceived = textSubmission.PointsReceived;
                submission.IsGraded = true;

                // Create a notification for the newly graded assignment
                GradedNotification notificationForStudent = new GradedNotification();
                
                // This is a text submission
                notificationForStudent.TextSubmission = submission;
                notificationForStudent.FileSubmission = null;
                notificationForStudent.IsFileSubmissionNotification = false;

                notificationForStudent.IsSeen = false; // The notification starts off not having been seen
                _context.GradedNotifications.Add(notificationForStudent);

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                _context.Attach(submission).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!textSubmissionExists(assignment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            else if(subType == "File")
            {
                FileSubmission submission = await _context.FileSubmissions.FirstOrDefaultAsync(s => s.Id == subId);
                submission.PointsReceived = textSubmission.PointsReceived;
                submission.IsGraded = true;

                // Create a notification for the newly graded assignment
                GradedNotification notificationForStudent = new GradedNotification();

                // This is a file submission
                notificationForStudent.TextSubmission = null;
                notificationForStudent.FileSubmission = submission;
                notificationForStudent.IsFileSubmissionNotification = true;

                notificationForStudent.IsSeen = false; // The notification starts off not having been seen
                _context.GradedNotifications.Add(notificationForStudent);

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                _context.Attach(submission).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!fileSubmissionExists(assignment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return RedirectToPage("/GradeAssignment", new { id = subId, subType = subType });

        }

        private bool textSubmissionExists(int id)
        {
            return _context.TextSubmissions.Any(s => s.Id == id);
        }
        private bool fileSubmissionExists(int id)
        {
            return _context.FileSubmissions.Any(s => s.Id == id);
        }

        public ActionResult OnPostDownloadFile(string fileName)
        {
            return File("/assignmentUploads/" + fileName, "application/octet-stream",
                       fileName);
        }

        // This method handles when the user clicks the red X on one of the notifications
        public async Task<IActionResult> OnGetRedX(int id, String subType, int notificationID, string type)
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
            return await OnGetAsync(id, subType); // Asynchronously return the OnGetAsync method
        }
    }
}
