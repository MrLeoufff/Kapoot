namespace Kapoot.Application.DTOs;

public record ProfileDto(UserDto User, int NbQuizzesCreated, int NbQuizzesHosted, int NbGamesPlayed);
