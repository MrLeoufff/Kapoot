<?php

declare(strict_types=1);

namespace App\Application\Service;

use App\Application\Repository\GameSessionRepositoryInterface;
use App\Application\Repository\PlayerRepositoryInterface;
use App\Application\Repository\QuizRepositoryInterface;
use App\Application\Repository\UserRepositoryInterface;

final class UserService
{
    public function __construct(
        private readonly UserRepositoryInterface $userRepository,
        private readonly QuizRepositoryInterface $quizRepository,
        private readonly GameSessionRepositoryInterface $gameSessionRepository,
        private readonly PlayerRepositoryInterface $playerRepository,
    ) {
    }

    /**
     * @return array{user: array, nbQuizzesCreated: int, nbQuizzesHosted: int, nbGamesPlayed: int}|null
     */
    public function getProfile(string $userId): ?array
    {
        $user = $this->userRepository->getById($userId);
        if ($user === null) {
            return null;
        }

        $quizzes = $this->quizRepository->getByOwnerId($userId);
        $hosted = $this->gameSessionRepository->getByHostId($userId);
        $played = $this->playerRepository->getByUserId($userId);

        return [
            'user' => [
                'id' => $user->getId(),
                'email' => $user->getEmail(),
                'pseudo' => $user->getPseudo(),
                'avatarUrl' => $user->getAvatarUrl(),
                'isAdmin' => $user->isAdmin(),
            ],
            'nbQuizzesCreated' => count($quizzes),
            'nbQuizzesHosted' => count($hosted),
            'nbGamesPlayed' => count($played),
        ];
    }
}
