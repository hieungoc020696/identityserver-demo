using IdentityServer4.AspNetIdentity;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using IdentityModel;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServerDemo
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEventService _events;
        private readonly ILogger<ResourceOwnerPasswordValidator<IdentityUser>> _logger;
        private readonly IStringLocalizer<ResourceOwnerPasswordValidator> _localizer;

        public ResourceOwnerPasswordValidator(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEventService events, ILogger<ResourceOwnerPasswordValidator<IdentityUser>> logger, IStringLocalizer<ResourceOwnerPasswordValidator> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _events = events;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByNameAsync(context.UserName);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, context.Password, true);
                if (result.Succeeded)
                {
                    var sub = await _userManager.GetUserIdAsync(user);

                    _logger.LogInformation("Credentials validated for username: {username}", context.UserName);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(context.UserName, sub, context.UserName, interactive: false));
                    context.Result = new GrantValidationResult(sub, OidcConstants.AuthenticationMethods.Password);
                    return;
                }

                if (result.IsLockedOut)
                {
                    _logger.LogInformation("Authentication failed for username: {username}, reason: locked out", context.UserName);
                    await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName, "locked out", interactive: false));
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, _localizer["User account locked out."]);
                    return;
                }

                if (result.IsNotAllowed)
                {
                    _logger.LogInformation("Authentication failed for username: {username}, reason: not allowed", context.UserName);
                    await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName, "not allowed", interactive: false));
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, _localizer["User account locked out."]);
                    return;
                }

                _logger.LogInformation("Authentication failed for username: {username}, reason: invalid credentials", context.UserName);
                await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName, "invalid credentials", interactive: false));
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, _localizer["Wrong password"]);
                return;
            }

            _logger.LogInformation("No user found matching username: {username}", context.UserName);
            await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName, "invalid username", interactive: false));
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, _localizer["No user found with this name or email address"]);
        }
    }
}
