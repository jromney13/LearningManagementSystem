using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace _5GuysLMS.Models
{
    public class User
    {
        public int Id { get; set; }
        [DisplayName("First Name")]
        public string FirstName { get; set; }
        [DisplayName("Last Name")]
        public string LastName { get; set; }
        [DataType(DataType.EmailAddress)]
        [DisplayName("Email Address")]
        public string EmailAddress { get; set; }
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
        [DataType(DataType.Date)]
        [DisplayName("Birth Date")]
        public DateTime Birthday { get; set; }
        [DisplayName("User Type")]
        public string UserType { get; set; }
        [DisplayName("Address Line 1")]
        public string AddressLineOne { get; set; }
        [DisplayName("Address Line 2")]
        public string AddressLineTwo { get; set; }
        [DisplayName("City")]
        public string City { get; set; }
        [DisplayName("State")]
        public string State { get; set; }
        [DisplayName("Zip")]
        public string Zip { get; set; }
        [DisplayName("Phone Number")]
        public string Phone { get; set; }
        [DisplayName("User Bio")]
        public string Bio { get; set; }
        [DisplayName("Social Link One")]
        public string LinkOne { get; set; }
        [DisplayName("Social Link Two")]
        public string LinkTwo { get; set; }
        [DisplayName("Social Link Three")]
        public string LinkThree { get; set; }
        [DisplayName("Profile Picture")]
        //navigation properties
        public ProfilePicture ProfilePicture { get; set; }

        public string getFullName() {
            return FirstName + " " + LastName;
        
        }
    }
}
