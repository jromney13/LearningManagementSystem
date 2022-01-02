using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _5GuysLMS.Models;
using Microsoft.AspNetCore.Http;

namespace _5GuysLMS.Pages
{
    public class SecurityQuestionsModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;
        public SecurityQuestionsModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }

        public User QueriedUser { get; set; }
        [BindProperty] public string AnswerInput1 { get; set; }
        [BindProperty] public string AnswerInput2 { get; set; }
        [BindProperty] public string AnswerInput3 { get; set; }
        [BindProperty] public string ErrorMessage { get; set; }
        public IList<SecurityQuestion> SecurityQuestions { get; set; }
        public IList<SecurityQuestionAnswer> SecurityQuestionAnswers { get; set; }

        public async Task<IActionResult> OnGetAsync(int userID)
        {
            if (userID != default)
            {
                // Grab the user and their corresponding security question answers from the DB
                QueriedUser = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();
                SecurityQuestionAnswers = await _context.SecurityQuestionsAnswers.
                    Include(sqa => sqa.SecurityQuestion).
                    Where(sqa => sqa.User == QueriedUser).
                    ToListAsync();

                // If the user has set up security question answers, find the related questions
                if (SecurityQuestionAnswers.Count > 0)
                {
                    SecurityQuestions = new List<SecurityQuestion>();
                    foreach (SecurityQuestionAnswer sqa in SecurityQuestionAnswers)
                    {
                        SecurityQuestions.Add(await _context.SecurityQuestions.Where(sq => sq == sqa.SecurityQuestion).FirstOrDefaultAsync());
                    }
                }
                return Page();
            }
            else // The userID is required for the page to work, so if it was set to default, return not found
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAsync(int userID)
        {
            // Grab the user and their corresponding security question answers from the DB
            QueriedUser = await _context.Users.Where(u => u.Id == userID).FirstOrDefaultAsync();
            SecurityQuestionAnswers = await _context.SecurityQuestionsAnswers.
                Include(sqa => sqa.SecurityQuestion).
                Where(sqa => sqa.User == QueriedUser).
                ToListAsync();

            // If the user has set up security question answers, find the related questions
            if (SecurityQuestionAnswers.Count > 0)
            {
                SecurityQuestions = new List<SecurityQuestion>();
                foreach (SecurityQuestionAnswer sqa in SecurityQuestionAnswers)
                {
                    SecurityQuestions.Add(await _context.SecurityQuestions.Where(sq => sq == sqa.SecurityQuestion).FirstOrDefaultAsync());
                }
            }

            // All questions must be answered to validate a user
            if (string.IsNullOrEmpty(AnswerInput1) || string.IsNullOrEmpty(AnswerInput2) || string.IsNullOrEmpty(AnswerInput3))
            {
                ErrorMessage = "Please answer all of the questions.";
                return Page();
            }
            // None of the questions can be answered incorrectly
            else if (SecurityQuestionAnswers[0].QuestionAnswer != AnswerInput1 ||
                     SecurityQuestionAnswers[1].QuestionAnswer != AnswerInput2 ||
                     SecurityQuestionAnswers[2].QuestionAnswer != AnswerInput3)
            {
                ErrorMessage = "One or more of your answers was incorrect. Please try again.";
                return Page();
            }
            // For security, before redirecting to the page where the user can reset their password, their ID is stored in the session
            // If the user's ID is not found in the session when they request the ResetPassword page, it will return not found
            // This prevents users from simply typing in the URL of the ResetPassword page manually and bypassing the validation
            HttpContext.Session.SetInt32("ResetPasswordValidatedUserID", QueriedUser.Id);
            return RedirectToPage("/ResetPassword", new { userID = QueriedUser.Id });
        }
    }
}

