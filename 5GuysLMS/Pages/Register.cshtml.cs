using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using _5GuysLMS.Data;
using _5GuysLMS.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace _5GuysLMS.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;

        [BindProperty]
        public string InvalidFormMessage { get; set; }
        public RegisterModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty, Required]
        public User User { get; set; }

        [BindProperty, Required]
        public string PasswordInput { get; set; }
        [BindProperty, Required]
        public string ConfirmPasswordInput { get; set; }
        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {

            //if password don't match, update  error message and return to page
            if (!PasswordInput.Equals(ConfirmPasswordInput)) {
                InvalidFormMessage = "Password Fields Must Match.";

                return Page();
            }
            //if user is less than 16 years old update error message and return to page

            if (CalculateAge(User.Birthday) < 16) {
                InvalidFormMessage = "Users must be 16 years old or older.";
                return Page();
            }
            //if user isn't a student or instructor go to error. they've messed with the select field
            if (User.UserType != "Instructor" && User.UserType != "Student") {

               return RedirectToPage("Error");
            }

            //final check if default validation has worked
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // query for user matching email address input
            var data = (from u in _context.Users
                        where u.EmailAddress.Equals(User.EmailAddress)
                        select u);

            //if there's more than 0 email returned by the query
            #pragma warning disable CA1827 // Do not use Count() or LongCount() when Any() can be used
            if (data.Count() > 0) {
            #pragma warning restore CA1827 // Do not use Count() or LongCount() when Any() can be used
                //inform user and return page
                InvalidFormMessage = "Email is already registered.";
                return Page();

            }


            //===End of Validation===


            //Hash our password input and put it into our new user Object
            SaltedHash saltedHash = Hasher.getSaltedHash(PasswordInput);
            User.HashedPassword = saltedHash.hash;
            User.Salt = saltedHash.salt;
            User.ProfilePicture = await  _context.ProfilePictures.FindAsync(9);

            if (User.UserType == "Instructor")
            {
                Instructor instructor = new Instructor();
                instructor.User = User;
                _context.Instructors.Add(instructor);
            }
            else
            {
                _context.Users.Add(User);
            }

            //Save our context 
            await _context.SaveChangesAsync();

            //get our id for our new user from the database
            var queryResults = (from u in _context.Users
                                where u.EmailAddress.Equals(User.EmailAddress)
                                select u);
            User queriedUser = queryResults.FirstOrDefault();


            if (queriedUser != default(User))//if queried user isn't default(kinda like null)
            {
                //Set our session values
                HttpContext.Session.SetInt32("UserID", queriedUser.Id);
                HttpContext.Session.SetString("UserType", queriedUser.UserType);
                //HttpContext.Session.SetString("UserProfilePicture", queriedUser.ProfilePicture.GenerateURL());
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
            else {

                return Page();
            }



        }

        /// <summary>
        /// Calculates Users Age in years given a inputed DateTime
        /// </summary>
        /// <param name="birthdayInput"></param>
        /// <returns>User's Age in Years</returns>
        private static int CalculateAge(DateTime birthdayInput) {
            
            //age is difference between now and birthday input
            int age = DateTime.Now.Year - birthdayInput.Year;
            //if they haven't had their birthday yet this year
            if (DateTime.Now.DayOfYear < birthdayInput.DayOfYear) {
                //they're actually one year younger
                age--;
            }
            return age;
        }
    }
}
