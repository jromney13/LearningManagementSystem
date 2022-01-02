using System;
using System.ComponentModel;

namespace _5GuysLMS.Models
{
    public class SecurityQuestion
    {
        public int Id { get; set; }
        [DisplayName("Question")]
        public string QuestionContent { get; set; }
    }
}
