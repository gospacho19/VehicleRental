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

        public required string Email { get; init; }
        public required string Phone { get; init; }
    }
}