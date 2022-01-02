using System;
using System.ComponentModel;

namespace _5GuysLMS.Models
{
    public class TextSubmission
    {
        public int Id { get; set; }
        [DisplayName("Text Content")]
        public string TextContent { get; set; }
        public bool IsGraded { get; set; }
        [DisplayName("Grade")]
        public int PointsReceived { get; set; }
        [DisplayName("Late")]
        public bool IsLate { get; set; }
        // Navigation Properties
        public User User { get; set; }
        public Assignment Assignment { get; set; }
    }
}
