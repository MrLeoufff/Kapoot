namespace Kapoot.Api.Requests;

public record RegisterRequest(string Email, string Password, string Pseudo);
public record LoginRequest(string Email, string Password);
