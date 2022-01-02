using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _5GuysLMS.Models;
using System.ComponentModel.DataAnnotations;
using _5GuysLMS.Data;

namespace _5GuysLMS.Pages
{
    public class AccountRecoveryModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;
        public AccountRecoveryModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }

        public User QueriedUser { get; set; }
        [BindProperty, Required] public string EmailInput { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }
        public IList<SecurityQuestionAnswer> SecurityQuestionAnswers { get; set; }

        public async Task<IActionResult> OnGetAsync(int? userID)
        {
            if (userID != null)
            {
                // Grab the user and their corresponding security question answers from the DB
                QueriedUser = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();
                SecurityQuestionAnswers = await _context.SecurityQuestionsAnswers.
                    Include(sqa => sqa.SecurityQuestion).
                    Where(sqa => sqa.User == QueriedUser).
                    ToListAsync();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? userID)
        {
            if (userID == null && string.IsNullOrEmpty(EmailInput))
            {
                ErrorMessage = "Please type your email in the textbox.";
                return Page();
            }
            else
            {
                // Look up the user in the DB based on the email input
                QueriedUser = await _context.Users.Where(u => u.EmailAddress == EmailInput).FirstOrDefaultAsync();

                if (QueriedUser != default(User)) // A user with the corresponding email was found in the DB
                {
                    SecurityQuestionAnswers = await _context.SecurityQuestionsAnswers.
                        Include(sqa => sqa.SecurityQuestion).
                        Where(sqa => sqa.User == QueriedUser).
                        ToListAsync();

                    // If the user has their security question answers set up, redirect them to the page where they can answer them
                    // Otherwise, redirect them back to this page
                    return SecurityQuestionAnswers.Count > 0 ?
                        RedirectToPage("/SecurityQuestions", new { userID = QueriedUser.Id }) :
                        RedirectToPage("/AccountRecovery", new { userID = QueriedUser.Id });
                }
                else // A user with the corresponding email was not found in the DB
                {
                    ErrorMessage = "The email that you entered does not exist within our system.";
                    return Page();
                }
            }
        }
    }
}
