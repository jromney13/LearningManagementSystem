using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using Newtonsoft.Json;
using _5GuysLMS.Data;
using _5GuysLMS.Data.Charge;

namespace _5GuysLMS.Pages.Users
{
    public class TestPaymentModel : PageModel
    {   [BindProperty]
       public string cardNumber { get; set; }
        [BindProperty]
        public string expMonth { get; set; }
        [BindProperty]
        public string expYear { get; set; }
        [BindProperty]
        public string cvc { get; set; }
        [BindProperty]
        public int amount { get; set; }

        [BindProperty]
        public string message { get; set; }
        public void OnGet()
        {


        }

        public async Task<IActionResult> OnPostPayment() {
            string publishableKey = "pk_test_51JespiIl7D8qhw61U0nzrJ3zwYNKXUV1MypGQW2G2uEbbh7W2Em20Ta2L21QWpNBXhUqdsuttEbvlQJ0FuTjYJG500sRvtl3D4";
            string secretKey = "sk_test_51JespiIl7D8qhw61xVvv0pI559rz4XucaeZ73FqRD1PsHe1y2eReQaLngBBPzoGjmmyBsC0Q883aPgLHRo7qXElZ00ix5mGFDk";

            //one client for each application lifetime
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + secretKey);
            //use the given information to create a token
           

            string tokenUrl = "https://api.stripe.com/v1/tokens";
            var tokenValues = new Dictionary<string, string> {

                {"card[number]", cardNumber},
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

                {"amount", amount.ToString()},
                {"currency", "usd" },
                { "description", "Test Charge"},
                {"source", token.id }
                
            };
            //encode the arguments
            var chargeContent = new FormUrlEncodedContent(chargeValues);
            //get response using url and arguments
            var response = await client.PostAsync(url, chargeContent);
            //convert response to string
            string responseString = await response.Content.ReadAsStringAsync();
            Charge charge = JsonConvert.DeserializeObject<Charge>(responseString);

            if (charge.paid) {
                message = "Payment Successful";
            }
            else {
                message = "Payment Failed";
            }

            //deserialize json

            return Page();



        }
    }
}
