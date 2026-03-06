using Kapoot.Application.DTOs;

namespace Kapoot.Application.Queries.GetProfile;

public record GetProfileResult(ProfileDto? Profile, bool Found);
