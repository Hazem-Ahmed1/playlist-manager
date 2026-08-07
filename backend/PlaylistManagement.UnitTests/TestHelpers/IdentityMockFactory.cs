using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PlaylistManagement.Api.Models;

namespace PlaylistManagement.UnitTests.TestHelpers
{
    /// <summary>
    /// UserManager/SignInManager have large constructors full of framework
    /// plumbing that AuthServiceTests doesn't care about. This factory
    /// builds mockable instances with everything except the store defaulted
    /// to null, which Moq/Identity tolerate for unit testing purposes.
    /// </summary>
    public static class IdentityMockFactory
    {
        public static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        public static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(UserManager<ApplicationUser> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            return new Mock<SignInManager<ApplicationUser>>(
                userManager, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);
        }
    }
}
