using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _5GuysLMS.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public ICollection<Course> Courses { get; set; }
    }
}
