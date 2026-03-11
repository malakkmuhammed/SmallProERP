using Microsoft.AspNetCore.Identity;
using SmallProERP.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.Entities
{
    public class User : IdentityUser<int>
    {
        
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int TenantId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        public Tenant? Tenant { get; set; }
        public User? Creator { get; set; }
    }
}
