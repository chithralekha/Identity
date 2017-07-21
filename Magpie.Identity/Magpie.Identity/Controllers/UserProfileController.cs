using System.Threading.Tasks;
using System.Web.Http;

namespace Magpie.Identity.Controllers
{
    public class UserProfileController : BaseApiController
    {
        [Authorize]
        [Route("user/profile")]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var user = await this.AppUserManager.FindByNameAsync(User.Identity.Name);

                if (user != null)
                {
                    var userProfile = this.TheModelFactory.Create(user);
                    
                    return Ok(userProfile);
                }
                return NotFound();
            }

            catch
            {
                return NotFound();
            }
        }
    }
}
    