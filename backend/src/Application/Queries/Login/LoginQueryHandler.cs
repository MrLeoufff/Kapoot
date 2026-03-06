using Kapoot.Application.DTOs;
using Kapoot.Application.Interfaces;

namespace Kapoot.Application.Queries.Login;

public class LoginQueryHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : ILoginQueryHandler
{
    public async Task<LoginResult?> HandleAsync(LoginQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Email) || string.IsNullOrWhiteSpace(query.Password))
            return null;

        var user = await userRepository.GetByEmailAsync(query.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !passwordHasher.VerifyPassword(query.Password, user.PasswordHash))
            return null;

        var dto = new UserDto(user.Id, user.Email, user.Pseudo, user.AvatarUrl, user.IsAdmin);
        return new LoginResult(dto);
    }
}
