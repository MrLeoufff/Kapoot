<?php

declare(strict_types=1);

namespace App\Application\Service;

use App\Application\Repository\GameSessionRepositoryInterface;
use App\Application\Repository\PlayerRepositoryInterface;
use App\Application\Repository\QuizRepositoryInterface;
use App\Entity\GameSession;
use App\Entity\Player;
use App\Enum\GameSessionStatus;
use App\Enum\QuizStatus;

final class GameSessionService
{
    public function __construct(
        private readonly GameSessionRepositoryInterface $gameSessionRepository,
        private readonly PlayerRepositoryInterface $playerRepository,
        private readonly QuizRepositoryInterface $quizRepository,
    ) {
    }

    /**
     * @return array{sessionId: string, code: string, quizId: string}
     */
    public function create(string $quizId, string $hostId): array
    {
        $quiz = $this->quizRepository->getById($quizId);
        if ($quiz === null) {
            throw new \RuntimeException('Quiz introuvable.');
        }
        if ($quiz->getStatus() !== QuizStatus::Published) {
            throw new \RuntimeException('Seul un quiz publié peut être joué.');
        }

        $session = new GameSession();
        $session->setQuizId($quizId);
        $session->setHostId($hostId);
        $this->gameSessionRepository->add($session);

        return [
            'sessionId' => $session->getId(),
            'code' => $session->getCode(),
            'quizId' => $session->getQuizId(),
        ];
    }

    /**
     * @return array{id: string, quizId: string, hostId: string, code: string, status: int, createdAt: string, startedAt: string|null, finishedAt: string|null}|null
     */
    public function getByCode(string $code): ?array
    {
        $session = $this->gameSessionRepository->getByCode(strtoupper($code));
        return $session === null ? null : $this->sessionToArray($session);
    }

    /**
     * @return list<array{id: string, pseudo: string}>
     */
    public function getPlayersByCode(string $code): array
    {
        $session = $this->gameSessionRepository->getByCode(strtoupper($code));
        if ($session === null) {
            return [];
        }
        $players = $this->playerRepository->getByGameSessionId($session->getId());
        $result = [];
        foreach ($players as $p) {
            if (!$p->hasLeft()) {
                $result[] = ['id' => $p->getId(), 'pseudo' => $p->getPseudo()];
            }
        }
        return $result;
    }

    /**
     * @return array{playerId: string, pseudo: string, sessionId: string}
     */
    public function join(string $code, string $pseudo, ?string $userId = null): array
    {
        $session = $this->gameSessionRepository->getByCode(strtoupper(trim($code)));
        if ($session === null) {
            throw new \RuntimeException('Partie introuvable.');
        }
        if ($session->getStatus() !== GameSessionStatus::Waiting) {
            throw new \RuntimeException('La partie a déjà commencé ou est terminée.');
        }
        $pseudo = trim($pseudo);
        if ($pseudo === '') {
            throw new \InvalidArgumentException('Le pseudo est obligatoire.');
        }

        $existingPlayers = $this->playerRepository->getByGameSessionId($session->getId());
        foreach ($existingPlayers as $p) {
            if (strcasecmp($p->getPseudo(), $pseudo) === 0) {
                throw new \RuntimeException('Ce pseudo est déjà pris dans cette partie.');
            }
            if ($userId !== null && $p->getUserId() === $userId) {
                throw new \RuntimeException('Vous avez déjà rejoint cette partie.');
            }
        }

        $player = new Player();
        $player->setGameSessionId($session->getId());
        $player->setPseudo($pseudo);
        $player->setUserId($userId);
        $this->playerRepository->add($player);

        return [
            'playerId' => $player->getId(),
            'pseudo' => $player->getPseudo(),
            'sessionId' => $session->getId(),
        ];
    }

    private function sessionToArray(GameSession $s): array
    {
        return [
            'id' => $s->getId(),
            'quizId' => $s->getQuizId(),
            'hostId' => $s->getHostId(),
            'code' => $s->getCode(),
            'status' => $s->getStatus()->value,
            'createdAt' => $s->getCreatedAt()->format(\DateTimeInterface::ATOM),
            'startedAt' => $s->getStartedAt()?->format(\DateTimeInterface::ATOM),
            'finishedAt' => $s->getFinishedAt()?->format(\DateTimeInterface::ATOM),
        ];
    }
}
