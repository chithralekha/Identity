using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace Magpie.Identity.Infrastructure
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength (256)]
        public string FirstName  { get; set;}

        [MaxLength(256)]
        public string LastName { get; set; }

        [MaxLength(128)]
        public string Manager { get; set; }
    
        public int? PhoneCarrierId { get; set; }

        public bool? IsActive { get; set; }

        //Rest of code is removed for brevity
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }
}