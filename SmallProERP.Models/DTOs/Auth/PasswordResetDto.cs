using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.Auth
{
    public class PasswordResetDto
    {
        public string Username { get; set; }
        public string NewPassword { get; set; }
    }
}
