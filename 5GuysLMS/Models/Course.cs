using System;
using System.ComponentModel;

namespace _5GuysLMS.Models
{
    public class Course
    {
        // Get-only property that returns the 24-hour StartTime TimeSpan as a string formatted with AM/PM
        public string StartTimeAMPM
        {
            get
            {
                return DateTime.Today.Add(StartTime).ToString("hh:mm tt");
            }
        }
        // Get-only property that returns the 24-hour EndTime TimeSpan as a string formatted with AM/PM
        public string EndTimeAMPM
        {
            get
            {
                return DateTime.Today.Add(EndTime).ToString("hh:mm tt");
            }
        }
        public int Id { get; set; }
        [DisplayName("Course Number")]
        public string CourseNumber { get; set; }
        [DisplayName("Course Department")]
        public string CourseDepartment { get; set; }
        [DisplayName("Course Title")]
        public string CourseTitle { get; set; }
        [DisplayName("Course Description")]
        public string CourseDesc { get; set; }
        [DisplayName("Meeting Days")]
        public string MeetDays { get; set; }
        [DisplayName("Meeting Location")]
        public string MeetingLocation { get; set; }
        [DisplayName("Start Time")]
        public TimeSpan StartTime { get; set; }
        [DisplayName("End Time")]
        public TimeSpan EndTime { get; set; }
        [DisplayName("Credit Hours")]
        public int CreditHours { get; set; }
        //Navigation Properties
        public Instructor Instructor { get; set; }
    }
}
