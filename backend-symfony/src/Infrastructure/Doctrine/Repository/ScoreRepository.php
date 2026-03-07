<?php

declare(strict_types=1);

namespace App\Infrastructure\Doctrine\Repository;

use App\Application\Repository\ScoreRepositoryInterface;
use App\Entity\Score;
use Doctrine\ORM\EntityManagerInterface;

final class ScoreRepository implements ScoreRepositoryInterface
{
    public function __construct(
        private readonly EntityManagerInterface $em,
    ) {
    }

    public function getByPlayerAndGameSession(string $playerId, string $gameSessionId): ?Score
    {
        return $this->em->getRepository(Score::class)->findOneBy(
            ['playerId' => $playerId, 'gameSessionId' => $gameSessionId]
        );
    }

    public function getByGameSessionId(string $gameSessionId): array
    {
        return $this->em->getRepository(Score::class)->findBy(
            ['gameSessionId' => $gameSessionId],
            ['totalPoints' => 'DESC']
        );
    }

    public function add(Score $score): void
    {
        $this->em->persist($score);
        $this->em->flush();
    }

    public function update(Score $score): void
    {
        $this->em->flush();
    }

    public function remove(Score $score): void
    {
        $this->em->remove($score);
        $this->em->flush();
    }
}
