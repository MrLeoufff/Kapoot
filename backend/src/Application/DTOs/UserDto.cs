namespace Kapoot.Application.DTOs;

public record UserDto(Guid Id, string Email, string Pseudo, string? AvatarUrl, bool IsAdmin);
