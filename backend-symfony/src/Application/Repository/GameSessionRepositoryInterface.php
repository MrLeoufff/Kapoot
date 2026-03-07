<?php

declare(strict_types=1);

namespace App\Application\Repository;

use App\Entity\GameSession;

interface GameSessionRepositoryInterface
{
    public function getById(string $id): ?GameSession;

    public function getByCode(string $code): ?GameSession;

    /** @return list<GameSession> */
    public function getByHostId(string $hostId): array;

    /** @return list<GameSession> */
    public function getByQuizId(string $quizId): array;

    public function add(GameSession $session): void;

    public function update(GameSession $session): void;

    public function remove(GameSession $session): void;
}
