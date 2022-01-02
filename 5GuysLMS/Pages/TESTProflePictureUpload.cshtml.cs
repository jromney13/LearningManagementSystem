using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using _5GuysLMS.Data;
using _5GuysLMS.Models;
using System.IO;

namespace _5GuysLMS.Pages
{
    public class TESTProflePictureUploadModel : PageModel
    {
        // good resources here:
        //https://www.learnrazorpages.com/razor-pages/forms/file-upload
        //http://www.binaryintellect.net/articles/2f55345c-1fcb-4262-89f4-c4319f95c5bd.aspx

        [BindProperty]
        public Microsoft.AspNetCore.Http.IFormFile UploadedFile { get; set; }


        private readonly _5GuysLMSContext _context;

        public TESTProflePictureUploadModel(_5GuysLMS.Data._5GuysLMSContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public ProfilePicture ProfilePicture { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            // set image title
            ProfilePicture.ImageTitle = UploadedFile.FileName;
            // get file extension
            string extension = Path.GetExtension(UploadedFile.FileName);
            //save to model
            ProfilePicture.FileExtension = extension;
            //use system.io to copy file contents to array for database
            MemoryStream ms = new();
            UploadedFile.CopyTo(ms);
            //save array to model
            ProfilePicture.ImageData = ms.ToArray();
            //release stream
            ms.Close();
            ms.Dispose();


            if (!ModelState.IsValid)
            {
                return Page();
            }
            

            //add profile picture to database
            _context.ProfilePictures.Add(ProfilePicture);
            await _context.SaveChangesAsync();

            
            //picture rendered on html side 
            return Page();
        }

       
    }
}
