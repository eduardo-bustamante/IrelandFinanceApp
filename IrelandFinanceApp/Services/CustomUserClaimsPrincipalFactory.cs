using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using IrelandFinanceApp.Models;

namespace IrelandFinanceApp.Services;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
{
    public CustomUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim("CultureCode", user.CultureCode ?? "en-IE"));
        identity.AddClaim(new Claim("CurrencySymbol", user.CurrencySymbol ?? "€"));

        return identity;
    }
}