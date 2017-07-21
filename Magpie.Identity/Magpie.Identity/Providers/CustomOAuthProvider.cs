using Magpie.Identity.Infrastructure;
using Magpie.Logging;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Magpie.Identity.Providers
{
    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
        protected ILog logger = new Logging.Logger.Log(typeof(CustomJwtFormat));

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {           

            var allowedOrigin = "*";

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

            var systemUser = await userManager.FindByNameAsync("System");

            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                logger.Error($"Login failed The user name or password is incorrect: {context.UserName}, Id: {user.Id}", systemUser.Id);
                return;
            }

            if ((!user.IsActive.HasValue) || (!user.IsActive.Value ))
            {
                context.SetError("invalid_grant", "The user is inactive");
                logger.Error($"Login failed The user is inactive: {context.UserName}, Id: {user.Id}", systemUser.Id);
                return;
            }

            //if (!user.EmailConfirmed)
            //{
            //    context.SetError("invalid_grant", "User did not confirm email.");
            //    logger.Error($"Login failed User did not confirm email: {context.UserName},  Id: {user.Id}", systemUser.Id);
            //    return;
            //}

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, "JWT");

            oAuthIdentity.AddClaims(ExtendedClaimsProvider.GetClaims(user));

            oAuthIdentity.AddClaims(RolesFromClaims.CreateRolesBasedOnClaims(oAuthIdentity));

            var ticket = new AuthenticationTicket(oAuthIdentity, null);

            context.Validated(ticket);

            if (ticket == null)
            {
                logger.Error($"Login failed The user name or password is incorrect: {context.UserName}", systemUser.Id);
            }
            else
            {
                List<Claim> claimsList = oAuthIdentity.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();             
                string rolesList = string.Join(",", claimsList.Select(r => r.Value).ToList());
                logger.Info($"Login Successful for {context.UserName} Roles: {rolesList}", systemUser.Id);
            }
        }
    }
}
