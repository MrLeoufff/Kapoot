<?php

declare(strict_types=1);

namespace App\Application\Repository;

use App\Entity\Score;

interface ScoreRepositoryInterface
{
    public function getByPlayerAndGameSession(string $playerId, string $gameSessionId): ?Score;

    /** @return list<Score> */
    public function getByGameSessionId(string $gameSessionId): array;

    public function add(Score $score): void;

    public function update(Score $score): void;

    public function remove(Score $score): void;
}
