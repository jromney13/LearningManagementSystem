using System;
using System.ComponentModel;

namespace _5GuysLMS.Models
{
    public class SecurityQuestionAnswer
    {
        public int Id { get; set; }
        [DisplayName("Answer")]
        public string QuestionAnswer { get; set; }
        // Navigation Properties
        public SecurityQuestion SecurityQuestion { get; set; }
        public User User { get; set; }
    }
}
