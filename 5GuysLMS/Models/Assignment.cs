using System;
using System.ComponentModel;

namespace _5GuysLMS.Models
{
    public enum SubmissionTypes{Text, File}
    public class Assignment
    {   
        public int Id { get; set; }
        [DisplayName("Assignment Title")]
        public string AssignmentTitle { get; set; }
        public string Description { get; set; }
        [DisplayName("Due Date")]
        public DateTime DueDate { get; set; }
        [DisplayName("Maximum Points")]
        public int MaxPoints { get; set; }
        [DisplayName("Submission Type")]
        public string SubmissionType { get; set; } // Enum is used when setting

        //Navigation Properties
        public Course Course {get; set;}
    }
}
