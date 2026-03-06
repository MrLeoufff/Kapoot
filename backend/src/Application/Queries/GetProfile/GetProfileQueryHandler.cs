using Kapoot.Application.DTOs;
using Kapoot.Application.Interfaces;

namespace Kapoot.Application.Queries.GetProfile;

public class GetProfileQueryHandler(
    IUserRepository userRepository,
    IQuizRepository quizRepository,
    IGameSessionRepository gameSessionRepository,
    IPlayerRepository playerRepository) : IGetProfileQueryHandler
{
    public async Task<GetProfileResult> HandleAsync(GetProfileQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user is null)
            return new GetProfileResult(null, false);

        var quizzes = await quizRepository.GetByOwnerIdAsync(query.UserId, cancellationToken);
        var hosted = await gameSessionRepository.GetByHostIdAsync(query.UserId, cancellationToken);
        var played = await playerRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        var userDto = new UserDto(user.Id, user.Email, user.Pseudo, user.AvatarUrl, user.IsAdmin);
        var profile = new ProfileDto(userDto, quizzes.Count, hosted.Count, played.Count);
        return new GetProfileResult(profile, true);
    }
}
