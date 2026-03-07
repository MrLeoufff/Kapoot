<?php

declare(strict_types=1);

namespace App\Infrastructure\Doctrine\Repository;

use App\Application\Repository\PlayerRepositoryInterface;
use App\Entity\Player;
use Doctrine\ORM\EntityManagerInterface;

final class PlayerRepository implements PlayerRepositoryInterface
{
    public function __construct(
        private readonly EntityManagerInterface $em,
    ) {
    }

    public function getById(string $id): ?Player
    {
        return $this->em->find(Player::class, $id);
    }

    public function getByGameSessionId(string $gameSessionId): array
    {
        return $this->em->getRepository(Player::class)->findBy(
            ['gameSessionId' => $gameSessionId],
            ['pseudo' => 'ASC']
        );
    }

    public function getByUserId(string $userId): array
    {
        return $this->em->getRepository(Player::class)->findBy(
            ['userId' => $userId],
            ['pseudo' => 'ASC']
        );
    }

    public function add(Player $player): void
    {
        $this->em->persist($player);
        $this->em->flush();
    }

    public function update(Player $player): void
    {
        $this->em->flush();
    }
}
