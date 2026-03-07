<?php

declare(strict_types=1);

namespace App\Application\Repository;

use App\Entity\Player;

interface PlayerRepositoryInterface
{
    public function getById(string $id): ?Player;

    /** @return list<Player> */
    public function getByGameSessionId(string $gameSessionId): array;

    /** @return list<Player> */
    public function getByUserId(string $userId): array;

    public function add(Player $player): void;

    public function update(Player $player): void;
}
