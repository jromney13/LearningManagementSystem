using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _5GuysLMS.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        //Navigation Properties
        public Course Course { get; set; }
        public User User { get; set; }
    }
}
