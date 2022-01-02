using _5GuysLMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using _5GuysLMS.Models;

namespace _5GuysLMS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;

        [BindProperty, Required]
        public string EmailInput { get; set; }

        [BindProperty, MinLength(1), Required]
        public string PasswordInput { get; set; }
        [BindProperty]
        public string InvalidLoginMessage { get; set; }

        /// <summary>
        /// Default Constructor. Params are provided by dependency Injection framework.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public IndexModel(ILogger<IndexModel> logger, _5GuysLMS.Data._5GuysLMSContext context)
        {
            _logger = logger;
            _context = context;

        }
        /// <summary>
        /// empty. perhaps once we're implementing sessions we can redirect to dashboard?
        /// </summary>
        public void OnGet()
        {

        }
        /// <summary>
        /// Runs asynchronously to get user by email for login.
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<Models.User>> GetUserByEmailAsync()
        {

            var data = from u in _context.Users.Include(u => u.ProfilePicture)
                       where u.EmailAddress.Equals(EmailInput)
                       select u;
            return await data.ToListAsync();

        }

        /// <summary>
        /// tries to login when user submits login form
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPost()
        {

            if (PasswordInput == null || EmailInput == null)
            {
                InvalidLoginMessage = "Invalid Input. Please provide Email and Password";

                _logger.LogInformation("Login Failure. Unfilled Form");

                return Page();
            }
            if (ModelState.IsValid)
            {

                //query for user by email asynchrounously.
                var data = await GetUserByEmailAsync();
                //grab the first or default user matching our eamail
                Models.User queriedUser = data.FirstOrDefault();



                if (queriedUser != default(Models.User))//if queried user isn't default
                {
                    //try hashing the password input with the salt from the db
                    string inputHash = Hasher.getSaltedHashSaltProvided(PasswordInput, queriedUser.Salt);
                    //if it matches our stored hash
                    if (inputHash.Equals(queriedUser.HashedPassword))
                    {
                        //Then our login worked. Do the stuff. 
                        _logger.LogInformation("Login Success");
                    }
                    else
                    {
                        InvalidLogin();
                        return Page();
                    }

                    //Set our session values
                    HttpContext.Session.SetInt32("UserID", queriedUser.Id);
                    HttpContext.Session.SetString("UserType", queriedUser.UserType);

                    //get our courses into the session
                    if (queriedUser.UserType == "Student")
                    {
                        // Get courses the user is enrolled in. Stores them in a list
                        List<Enrollment> EnrolledCourses = await _context.Enrollments.
                              Include(e => e.User).
                              Include(e => e.Course).
                              Where(e => e.User.Id == HttpContext.Session.GetInt32("UserID")).
                              ToListAsync();

                        // Get all unseen created notifications and store them in a list
                        List<CreatedNotification> UnseenCreatedNotifications = await _context.CreatedNotifications.
                            Include(cn => cn.Assignment).
                            Where(cn => cn.User.Id == HttpContext.Session.GetInt32("UserID")).
                            Where(cn => !cn.IsSeen).
                            ToListAsync();

                        // Get all unseen graded notifications and store them in a list
                        List<GradedNotification> UnseenGradedNotifications = await _context.GradedNotifications.
                            Include(gn => gn.FileSubmission).
                            Include(gn => gn.TextSubmission).
                            Include(gn => gn.FileSubmission.Assignment).
                            Include(gn => gn.TextSubmission.Assignment).
                            Where(gn => gn.IsFileSubmissionNotification ? gn.FileSubmission.User.Id == HttpContext.Session.GetInt32("UserID") : gn.TextSubmission.User.Id == HttpContext.Session.GetInt32("UserID")).
                            Where(gn => !gn.IsSeen).
                            ToListAsync();

                        // Store the complex data in the session
                        HttpContext.Session.SetComplexData("CourseList", EnrolledCourses);
                        HttpContext.Session.SetComplexData("CreatedNotifications", UnseenCreatedNotifications);
                        HttpContext.Session.SetComplexData("GradedNotifications", UnseenGradedNotifications);
                    }
                }
                //set our session profile picture url 
                if (queriedUser.ProfilePicture != null)
                {
                    HttpContext.Session.SetString("UserProfilePicture", queriedUser.ProfilePicture.GenerateURL());
                }
                //or set a default picture if the user doesn't have one yet
                else
                {
                    //HttpContext.Session.SetString("UserProfilePicture", _context.ProfilePictures.Find(9).GenerateURL());
                }
                return RedirectToPage("Dashboard");

            }
            else
            {

                InvalidLoginMessage = "Invalid Input. Please provide Email and Password";

                _logger.LogInformation("Login Failure. Invalid Form");

                return Page();
            }



        }

        private void InvalidLogin()
        {
            // communicate to user that password/user was invalid
            InvalidLoginMessage = "Invalid Username or Password";
            //return current page if false
            _logger.LogInformation("Login Failure. Invalid Email/Password");
        }

        // This method handles when the user clicks the red X on one of the notifications
        public void OnGetRedX(int notificationID, string type)
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
            OnGet(); // Synchronously call the OnGet method
        }
    }
}
