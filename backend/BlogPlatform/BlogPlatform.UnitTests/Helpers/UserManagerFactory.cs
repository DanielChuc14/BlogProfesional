using BlogPlatform.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace BlogPlatform.UnitTests.Helpers;

/// <summary>
/// Factory para crear un UserManager mockeado.
///
/// UserManager<T> es una clase concreta (no una interface), pero todos sus métodos públicos
/// son virtual, lo que permite a NSubstitute interceptarlos. El único parámetro realmente
/// obligatorio es IUserStore — el resto acepta null.
/// </summary>
public static class UserManagerFactory
{
    public static UserManager<ApplicationUser> Create()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();

        // Substitute.For<UserManager<T>> en lugar de new UserManager<T>:
        // UserManager es una clase concreta, pero sus métodos son virtual.
        // NSubstitute solo puede interceptar llamadas en substitutes —
        // si usamos `new`, .Returns() lanza CouldNotSetReturnDueToNoLastCallException.
#pragma warning disable CS8625
        return Substitute.For<UserManager<ApplicationUser>>(
            store,
            null, null, null, null, null, null, null, null);
#pragma warning restore CS8625
    }
}
