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
using System.Net.Http;
using Newtonsoft.Json;
using _5GuysLMS.Data.Charge;

namespace _5GuysLMS.Pages
{
    public class TuitionModel : PageModel
    {
        private readonly _5GuysLMSContext _context;

        [BindProperty] public User myUser { get; set; }
        [BindProperty] public List<Enrollment> studentEnrollments { get; set; }
        [BindProperty] public Course course { get; set; }
        [BindProperty] public Int32 owed { get; set; }
        [BindProperty] public string ccn { get; set; }
        [BindProperty] public string expMonth { get; set; }
        [BindProperty] public string expYear { get; set; }
        [BindProperty] public string cvc { get; set; }
        [BindProperty] public int amount { get; set; }
        [BindProperty] public string message { get; set; }
        public TuitionModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            //get our id out of our session
            int? id = HttpContext.Session.GetInt32("UserID");
            // if there wasn't a id in the session, return a 404
            if (id == null)
            {
                return NotFound();
            }

            
            //Gets all student enrollments
            studentEnrollments = _context.Enrollments.
                Include(e => e.User).
                Include(e => e.Course).
                Where(t => t.User.Id == id).ToList();

            //loops through student's enrollments and adds up the credit hours to get the tuition
            if (studentEnrollments.Count != 0)
            {
                foreach (var item in studentEnrollments)
                {
                    course = _context.Courses.FirstOrDefault(c => c.Id == item.Course.Id);
                     owed += (100 * course.CreditHours);
                    
                };

                //subtracts payments from owed
                List<Payment> studentPayments = await _context.Payments
                        .Include(p => p.Student)
                        .Where(p => p.Student.Id == id)
                        .ToListAsync();

                foreach (Payment payment in studentPayments)
                {
                    owed -= (int)payment.PaymentAmount;
                }
               
            }


            //save our changes to the database
            await _context.SaveChangesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostPayment()
        {
            string publishableKey = "pk_test_51JespiIl7D8qhw61U0nzrJ3zwYNKXUV1MypGQW2G2uEbbh7W2Em20Ta2L21QWpNBXhUqdsuttEbvlQJ0FuTjYJG500sRvtl3D4";
            string secretKey = "sk_test_51JespiIl7D8qhw61xVvv0pI559rz4XucaeZ73FqRD1PsHe1y2eReQaLngBBPzoGjmmyBsC0Q883aPgLHRo7qXElZ00ix5mGFDk";

            //one client for each application lifetime
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + secretKey);
            //use the given information to create a token


            string tokenUrl = "https://api.stripe.com/v1/tokens";
            var tokenValues = new Dictionary<string, string> {

                {"card[number]", ccn},
                {"card[exp_month]", expMonth },
                {"card[exp_year]", expYear},
                {"card[cvc]", cvc}

            };

            var tokenContent = new FormUrlEncodedContent(tokenValues);
            var tokenResponse = await client.PostAsync(tokenUrl, tokenContent);
            //convert response to string
            string tokenResponseString = await tokenResponse.Content.ReadAsStringAsync();

            //convert from json
            Token token = JsonConvert.DeserializeObject<Token>(tokenResponseString);




            //set the url
            string url = "https://api.stripe.com/v1/charges";
            //set our arguments
            var chargeValues = new Dictionary<string, string> {

                {"amount", (amount * 100).ToString()},
                {"currency", "usd" },
                { "description", "5 Guys LMS Tuition"},
                {"source", token.id }

            };
            //encode the arguments
            var chargeContent = new FormUrlEncodedContent(chargeValues);
            //get response using url and arguments
            var response = await client.PostAsync(url, chargeContent);
            //convert response to string
            string responseString = await response.Content.ReadAsStringAsync();
            Charge charge = JsonConvert.DeserializeObject<Charge>(responseString);

            if (charge.paid)
            {
                message = "Payment Successful";
                myUser = await _context.Users.FindAsync(HttpContext.Session.GetInt32("UserID"));
                Payment newPayment = new Payment();
                newPayment.PaymentAmount = amount;
                newPayment.Student = myUser;
                await _context.Payments.AddAsync(newPayment);
                await _context.SaveChangesAsync();
            }
            else
            {
                message = "Payment Failed";
            }

            //deserialize json
            return await OnGetAsync();
        }

        // This method handles when the user clicks the red X on one of the notifications
        public async Task<IActionResult> OnGetRedX(int notificationID, string type)
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
            return await OnGetAsync(); // Asynchronously return the OnGetAsync method
        }
    }
}
