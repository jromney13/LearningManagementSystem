using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _5GuysLMS.Data
{
    public class SaltedHash
    {
        public string hash { get; set; }
        public string salt { get; set; }

        public SaltedHash(string hash, string salt) {
            this.hash = hash;
            this.salt = salt;   
        }
    }
}
