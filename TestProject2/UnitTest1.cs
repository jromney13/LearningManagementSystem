using _5GuysLMS.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _5GuysLMS.Data;
using Microsoft.AspNetCore.Http;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;


namespace TestProject2
{
    [TestClass]
    public class UnitTest1
    {
        private _5GuysLMSContext _context;
        public Course course { get; set; }
        public IList<Assignment> Assignments { get; set; }

        [TestMethod]
        public void addAssignmentTest() //What are we testing?
        {
            //connect to the CourseDahsboard page
            DbContextOptions<_5GuysLMSContext> options = new DbContextOptions<_5GuysLMSContext>();
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder(options);
            SqlServerDbContextOptionsExtensions.UseSqlServer(builder, "Data Source=titan.cs.weber.edu,10433;Initial Catalog=LMSRepos;User ID=LMSRepos;Password=5GuysRepos2", null);
            _context = new _5GuysLMSContext((DbContextOptions<_5GuysLMSContext>)builder.Options);



            _5GuysLMS.Pages.CourseDashboardModel courseDashboard = new _5GuysLMS.Pages.CourseDashboardModel(_context);


            //get the number of assignments in this course
            var N = _context.Assignments.Where(a => a.Course.Id == 10).ToList().Count;
            //get course
            course = _context.Courses.Include(i => i.Instructor).FirstOrDefault(a => a.Id == 10);

    
            // preparation or set up

            //create a new assignment
            Assignment assignment = new Assignment();
            assignment.AssignmentTitle = "Show and Tell";
            assignment.Description = "Bring your favorite thing to class and show it to everyone";
            assignment.DueDate = DateTime.Now;
            assignment.MaxPoints = 100;
            assignment.SubmissionType = "Text";
            assignment.Course = course;

            // Perform the operations
            courseDashboard.AddAssignment(assignment);

            // verify and interpret the results

            //get the number of assignments in the course with new added course
            var newN = _context.Assignments.Where(a => a.Course.Id == 10).ToList().Count;
            //prove that there has been an added course
            Assert.IsTrue(newN == N+1);
           
         }

        [TestMethod]
        public void addCourseTest() //What are we testing?
        {
            //connect to the AddCourse page
            DbContextOptions<_5GuysLMSContext> options = new DbContextOptions<_5GuysLMSContext>();
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder(options);
            SqlServerDbContextOptionsExtensions.UseSqlServer(builder, "Data Source=titan.cs.weber.edu,10433;Initial Catalog=LMSRepos;User ID=LMSRepos;Password=5GuysRepos2", null);
            _context = new _5GuysLMSContext((DbContextOptions<_5GuysLMSContext>)builder.Options);


            _5GuysLMS.Pages.AddCourseModel addCourseModel = new _5GuysLMS.Pages.AddCourseModel(_context);


            //get the number of courses an instructor is teaching
            var N = _context.Courses.Where(a => a.Instructor.Id == 16).ToList().Count;

            //get the Instructor
            Instructor instructor = _context.Instructors.Where(i => i.Id == 16).FirstOrDefault();

            // preparation or set up

            //create a new course
            Course course = new Course();
            course.Instructor = instructor;
            course.MeetDays = "TR";
            course.MeetingLocation = "Elizabeth Hall";
            course.StartTime = new TimeSpan(0, 50, 0);
            course.EndTime = new TimeSpan(0, 50, 0);
            course.CreditHours = 2;
            course.CourseNumber = "3320";
            course.CourseDepartment = "CS";
            course.CourseTitle = "Advanced Computer Networking";
            course.CourseDesc = "A class about networking";

            // Perform the operations
            addCourseModel.AddCourse(course);

            // verify and interpret the results

            //get the number of courses that the instructor is teaching
            var newN = _context.Courses.Where(a => a.Instructor.Id == 16).ToList().Count;
            //prove that there has been an added course
            Assert.IsTrue(newN == N + 1);

        }

        [TestMethod]
        public void studentAddCourseTest() //What are we testing?
        {
            //connect to db
            DbContextOptions<_5GuysLMSContext> options = new DbContextOptions<_5GuysLMSContext>();
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder(options);
            SqlServerDbContextOptionsExtensions.UseSqlServer(builder, "Data Source=titan.cs.weber.edu,10433;Initial Catalog=LMSRepos;User ID=LMSRepos;Password=5GuysRepos2", null);
            _context = new _5GuysLMSContext((DbContextOptions<_5GuysLMSContext>)builder.Options);

            // preparation or set up

            //used variables 
            //Used students user id
            int iSID = 41;
            //Used course id
            int iCID = 30;
            //User that is being tested
            User u = _context.Users.Where(u => u.Id == iSID).FirstOrDefault();
            //Course that is being added 
            Course c = _context.Courses.Where(c => c.Id == iCID).FirstOrDefault();
            //The number of enrollments at the start
            int iN = _context.Enrollments.Where(u => u.User.Id == iSID).ToList().Count;
            //Number of enrollments at end 
            int iN2;
            //Enrollment being added 
            Enrollment e = new Enrollment();

            //Set up enrollment and add it to the db
            e.User = u;
            e.Course = c;
            // Perform the operations
            _context.Enrollments.Add(e);
            _context.SaveChanges();

            //Get number of enrollments at the end 
            iN2 = _context.Enrollments.Where(u => u.User.Id == iSID).ToList().Count;

            //Remove the enrollemnt after test 
            _context.Enrollments.Remove(e);
            _context.SaveChanges();

            // verify and interpret the results
            Assert.IsTrue(iN2 == iN + 1);
        }

        [TestMethod]
        public void submitAssignmentTest() // We are testing that a student can submit an assignment whether it is a file type or text type
        {
            // Connect to the DB
            DbContextOptions<_5GuysLMSContext> options = new DbContextOptions<_5GuysLMSContext>();
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder(options);
            SqlServerDbContextOptionsExtensions.UseSqlServer(builder, "Data Source=titan.cs.weber.edu,10433;Initial Catalog=LMSRepos;User ID=LMSRepos;Password=5GuysRepos2", null);
            _context = new _5GuysLMSContext((DbContextOptions<_5GuysLMSContext>)builder.Options);

            // Preparation & set up

            // The student must be enrolled in at least one course for the test and their course must have at least one text assignment and at least one file assignment for the test to work properly
            int studentID = 11; // Student with Id 11 fits the above criteria
            // Find the first enrollment of this student
            Enrollment e = _context.Enrollments.Include(e => e.User).Include(e => e.Course).Where(e => e.User.Id == studentID).FirstOrDefault();
            Course c = e.Course; // The course that the student is enrolled in
            // Find the last file assignment that this student was assigned
            Assignment aFile = _context.Assignments.Where(aF => aF.Course == c && aF.SubmissionType == "File").OrderBy(aF => aF.Id).LastOrDefault();
            // Find the last text assignment that this student was assigned
            Assignment aText = _context.Assignments.Where(aT => aT.Course == c && aT.SubmissionType == "Text").OrderBy(aF => aF.Id).LastOrDefault();

            // Find how many file submissions and text submissions the user had before
            int fileSubmissionCountBefore = _context.FileSubmissions.Where(fs => fs.User == e.User && fs.Assignment == aFile).Count();
            int textSubmissionCountBefore = _context.TextSubmissions.Where(ts => ts.User == e.User && ts.Assignment == aText).Count();

            // Create a new file submission for the student to submit
            FileSubmission fs = new FileSubmission
            {
                fileName = "NotARealAssignment",
                savedName = "NotARealAssignment123",
                IsGraded = false,
                IsLate = DateTime.Now > aFile.DueDate,
                Assignment = aFile,
                User = e.User
            };

            // Create a new text submission for the student to submit
            TextSubmission ts = new TextSubmission
            {
                TextContent = "This is an assignment that is being added in the submitAssignmentTest() method",
                IsGraded = false,
                IsLate = DateTime.Now > aFile.DueDate,
                Assignment = aText,
                User = e.User
            };

            // Add the newly-created file submission and text submission to the DB
            _context.FileSubmissions.Add(fs);
            _context.TextSubmissions.Add(ts);
            _context.SaveChanges(); // Save the changes

            // Get number of file submissions and text submissions at the end 
            int fileSubmissionCountAfter = _context.FileSubmissions.Where(fs => fs.User == e.User && fs.Assignment == aFile).Count();
            int textSubmissionCountAfter = _context.TextSubmissions.Where(ts => ts.User == e.User && ts.Assignment == aText).Count();

            // Remove the file submission and text submisson after the test
            _context.FileSubmissions.Remove(fs);
            _context.TextSubmissions.Remove(ts);
            _context.SaveChanges();

            // Verify and interpret the results
            Assert.IsTrue(fileSubmissionCountAfter == fileSubmissionCountBefore + 1);
            Assert.IsTrue(textSubmissionCountAfter == textSubmissionCountBefore + 1);
        }

        [TestMethod]
        public void uiEnrollInClassTest()
        {
            // Setup
            IWebDriver driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            //Navigates to homepage and finds email and password input boxes
            driver.Navigate().GoToUrl("https://localhost:44341/");
            IWebElement emailInputBox = driver.FindElement(By.Id("EmailInput"));
            IWebElement passwordInputBox = driver.FindElement(By.Id("PasswordInput"));
            
            //Fills email and passsword boxes
            emailInputBox.SendKeys("jromney13@gmail.com");
            passwordInputBox.SendKeys("pass123");

            //Finds and clicks login button
            IWebElement loginButton = driver.FindElement(By.XPath("/html/body/div/main/div/div[2]/form/input[3]"));
            loginButton.Click();

            // Gets the count of cards listed on the student dashboard
            IList<IWebElement> cardsPreAdd = driver.FindElements(By.CssSelector(".card"));
            int preAddCount = cardsPreAdd.Count();

            // Navigates to the enroll page
            IWebElement enrollLink = driver.FindElement(By.XPath("/html/body/header/nav/div/div/ul/li[4]/a"));
            enrollLink.Click();

            // Clicks the add course button for Integral Calculus to enroll in course
            IWebElement addCourseButton = driver.FindElement(By.XPath("/html/body/div/main/table/tbody/tr[4]/td[11]/form/button"));
            addCourseButton.Click();

            // Navigates back to the home page
            IWebElement homeButton = driver.FindElement(By.XPath("/html/body/header/nav/div/div/ul/li[1]/a"));
            homeButton.Click();

            // Gets the count of cards on listed on the student dashboard after addition
            IList<IWebElement> cardsPostAdd = driver.FindElements(By.CssSelector(".card"));
            int postAddCount = cardsPostAdd.Count();

            // Checks to see if the card count after enrollment is one more than pre-enrollment
            Assert.AreEqual((preAddCount + 1), postAddCount);

            // Clean Up
            enrollLink = driver.FindElement(By.XPath("/html/body/header/nav/div/div/ul/li[4]/a"));
            enrollLink.Click();

            addCourseButton = driver.FindElement(By.XPath("/html/body/div/main/table/tbody/tr[4]/td[11]/form/button"));
            addCourseButton.Click();

            driver.Quit();
        }


    }
}
