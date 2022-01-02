using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _5GuysLMS.Data;
using _5GuysLMS.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace _5GuysLMS.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;
        public ResetPasswordModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }

        public User QueriedUser { get; set; }
        [BindProperty, Required] public string PasswordInput { get; set; }
        [BindProperty, Required] public string ConfirmPasswordInput { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }
        public bool IsPasswordResetSuccessfully { get; set; }


        public async Task<IActionResult> OnGetAsync(int userID)
        {
            if (userID != default)
            {
                // Grab the user from the DB
                QueriedUser = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();

                // The user must have validated that they are truly the user they are claiming to be by answering the security questions
                // If they did validate themself properly, their ID will be stored in the session value ResetPasswordValidatedUserID
                // This validated ID stored in the session must match the one of the user that they are querying
                if(HttpContext.Session.GetInt32("ResetPasswordValidatedUserID") != null && HttpContext.Session.GetInt32("ResetPasswordValidatedUserID") == QueriedUser.Id)
                {
                    return Page();
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int userID)
        {
            // Grab the user from the DB
            QueriedUser = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();
            
            // The new password needs to be typed in both text boxes
            if (string.IsNullOrEmpty(PasswordInput) || string.IsNullOrEmpty(ConfirmPasswordInput))
            {
                ErrorMessage = "Please type your password in the textboxes.";
                return Page();
            }

            // If the entered passwords do not match, update error message and return to page
            if (!PasswordInput.Equals(ConfirmPasswordInput))
            {
                ErrorMessage = "Password Fields Must Match.";
                return Page();
            }

            // Hash our password input and put it into the QueriedUser object
            SaltedHash saltedHash = Hasher.getSaltedHash(PasswordInput);
            QueriedUser.HashedPassword = saltedHash.hash;
            QueriedUser.Salt = saltedHash.salt;

            await _context.SaveChangesAsync(); // Save the new password information for the user to the DB
            
            // Indicate that the password has been successfully reset and return the page
            IsPasswordResetSuccessfully = true;
            HttpContext.Session.Clear(); // Clear the session for security purposes
            return Page();
        }
    }
}
