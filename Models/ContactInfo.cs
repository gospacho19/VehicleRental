using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxuryCarRental.Models
{
    public class ContactInfo
    {
        public ContactInfo() { }
        // Change them to `get; set;`:
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}