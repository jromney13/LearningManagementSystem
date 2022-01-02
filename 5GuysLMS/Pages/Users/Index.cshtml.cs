using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _5GuysLMS.Data;
using _5GuysLMS.Models;

namespace _5GuysLMS.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;

        public IndexModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }

        public IList<User> User { get;set; }

        public async Task OnGetAsync()
        {
            User = await _context.Users.ToListAsync();
        }
    }
}
