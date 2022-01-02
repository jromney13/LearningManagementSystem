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

namespace _5GuysLMS.Pages
{
    public enum Departments { CS, ENGL, MATH, PHYS, WEB }

    public class AddCourseModel : PageModel
    {
        private readonly _5GuysLMS.Data._5GuysLMSContext _context;

        [BindProperty] public Instructor instructor { get; set; }
        [BindProperty] public Course AddedCourse { get; set; }
        [BindProperty] public List<Course> courseList { get; set; }

        public AddCourseModel(_5GuysLMS.Data._5GuysLMSContext context)
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

            //gets the instructor object for this user
            instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == id);

            //Gets courses that are taught by this instructor and puts them into a list
            courseList = _context.Courses.Where(t => t.Instructor.Id == instructor.Id).ToList();

            //save our changes to the database
            await _context.SaveChangesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {   //Edited user's state isn't kept from the request. If it wasn't addressed in the html it's set to null. We also can't hide it client side for data security reasons. 
            //get our id out of our session
            int? id = HttpContext.Session.GetInt32("UserID");

            Instructor instructor2 = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == id);

            //Sets the Course object's instructor to user
            AddedCourse.Instructor = instructor2;

            //Adds new course to database
            AddCourse(AddedCourse);

            return RedirectToPage("/AddCourse");
        }

        public async Task<IActionResult> OnPostDelete(int? Id)
        {
          
            if (Id == null)
            {
                return NotFound();
            }

            Course DeleteCourse = await _context.Courses.FindAsync(Id);

            if (DeleteCourse != null)
            {
                _context.Courses.Remove(DeleteCourse);
                _context.Enrollments.RemoveRange(_context.Enrollments.Where(e=>e.Course == DeleteCourse));
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/AddCourse");

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

        public void AddCourse(Course course)
        {
            //Adds new course to database
            _context.Courses.Add(course);
            _context.SaveChanges();
        }
    }
}
