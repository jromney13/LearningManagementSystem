using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace _5GuysLMS.Models
{
    public class Payment
    {
        public int Id { get; set; }
        [Column(TypeName = "money")]
        public decimal PaymentAmount { get; set; }
        public User Student { get; set; }
    }
}
