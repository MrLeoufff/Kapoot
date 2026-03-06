using Kapoot.Application.DTOs;
using Kapoot.Application.Interfaces;
using Kapoot.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Kapoot.Application.Commands.Register;

public class RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IConfiguration configuration)
    : IRegisterCommandHandler
{
    public async Task<RegisterResult> HandleAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("L'email est obligatoire.", nameof(command.Email));
        if (string.IsNullOrWhiteSpace(command.Password))
            throw new ArgumentException("Le mot de passe est obligatoire.", nameof(command.Password));
        if (string.IsNullOrWhiteSpace(command.Pseudo))
            throw new ArgumentException("Le pseudo est obligatoire.", nameof(command.Pseudo));

        var email = command.Email.Trim().ToLowerInvariant();
        if (await userRepository.GetByEmailAsync(email, cancellationToken) is not null)
            throw new InvalidOperationException("Un compte existe déjà avec cet email.");

        var initialAdminEmail = configuration["Admin:InitialAdminEmail"]?.Trim().ToLowerInvariant();
        var isAdmin = !string.IsNullOrEmpty(initialAdminEmail) && initialAdminEmail == email;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.HashPassword(command.Password),
            Pseudo = command.Pseudo.Trim(),
            AvatarUrl = null,
            IsAdmin = isAdmin,
            DateCreated = DateTime.UtcNow
        };

        var saved = await userRepository.AddAsync(user, cancellationToken);
        var dto = new UserDto(saved.Id, saved.Email, saved.Pseudo, saved.AvatarUrl, saved.IsAdmin);
        return new RegisterResult(dto);
    }
}
