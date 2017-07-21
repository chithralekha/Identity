using Magpie.Identity.Infrastructure;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Routing;

namespace Magpie.Identity.Models
{
    public class ModelFactory
    {
        private UrlHelper _UrlHelper;
        private ApplicationUserManager _AppUserManager;

        public ModelFactory(HttpRequestMessage request, ApplicationUserManager appUserManager)
        {
            _UrlHelper = new UrlHelper(request);
            _AppUserManager = appUserManager;
        }

        public UserReturnModel Create(ApplicationUser appUser)
        {
            return new UserReturnModel
            {
                // Url = _UrlHelper.Link("GetUserById", new { id = appUser.Id }),
                Id = appUser.Id,
                UserName = appUser.UserName,
                FullName = string.Format("{0} {1}", appUser.FirstName, appUser.LastName),
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email,
                EmailConfirmed = appUser.EmailConfirmed,
                Manager = appUser.Manager,
                PhoneNumber = appUser.PhoneNumber,
                PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
                PhoneCarrierId = appUser.PhoneCarrierId,
                IsActive = appUser.IsActive,
                securityOptions = CreateSecurityOpitions(appUser),
                Roles = _AppUserManager.GetRolesAsync(appUser.Id).Result,
               // Claims = _AppUserManager.GetClaimsAsync(appUser.Id).Result
            };
        }
        
        public SecurityOptionsReturnModel CreateSecurityOpitions(ApplicationUser appUser)
        {
            return new SecurityOptionsReturnModel
            {
                TwoFactorEnabled = appUser.TwoFactorEnabled,
                LockoutEndDateUtc = appUser.LockoutEndDateUtc,
                LockoutEnabled = appUser.LockoutEnabled,
                AccessFailedCount = appUser.AccessFailedCount
            };
        }

        public RoleReturnModel Create(IdentityRole appRole)
        {

            return new RoleReturnModel
            {
                Url = _UrlHelper.Link("GetRoleById", new { id = appRole.Id }),
                Id = appRole.Id,
                Name = appRole.Name
            };
        }
    }

    public class UserReturnModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Manager { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public int? PhoneCarrierId { get; set; }
        public bool? IsActive { get; set; }
        public IList<string> Roles { get; set; }
        public SecurityOptionsReturnModel securityOptions { get; set; }
 

        //  public IList<System.Security.Claims.Claim> Claims { get; set; }
    }

    public class SecurityOptionsReturnModel
    {
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
    }

    public class RoleReturnModel
    {
        public string Url { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}