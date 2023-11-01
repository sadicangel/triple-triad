using Microsoft.AspNetCore.Identity;

namespace TripleTriad;
public sealed class User : IdentityUser<Guid>
{
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    public override string UserName { get => base.UserName; set => base.UserName = value; }
    public override string Email { get => base.Email; set => base.Email = value; }
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
#pragma warning restore CS8603 // Possible null reference return.
    public bool IsActive { get; set; }
}

public sealed class UserClaim : IdentityUserClaim<Guid> { }
public sealed class UserLogin : IdentityUserLogin<Guid> { }
public sealed class UserToken : IdentityUserToken<Guid> { }