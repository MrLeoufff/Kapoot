namespace Kapoot.Application.Commands.SetUserAdmin;

public record SetUserAdminCommand(Guid UserId, bool IsAdmin);
