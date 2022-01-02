using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace _5GuysLMS.Models
{
    public class FileSubmission
    {
        public int Id { get; set; }
        [DisplayName("File Name")]
        public string fileName { get; set; }
        [DisplayName("Saved Name")]
        public string savedName { get; set; }
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
