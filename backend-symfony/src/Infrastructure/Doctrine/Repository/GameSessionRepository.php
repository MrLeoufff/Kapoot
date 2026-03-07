<?php

declare(strict_types=1);

namespace App\Infrastructure\Doctrine\Repository;

use App\Application\Repository\GameSessionRepositoryInterface;
use App\Entity\GameSession;
use Doctrine\ORM\EntityManagerInterface;

final class GameSessionRepository implements GameSessionRepositoryInterface
{
    private const CODE_CHARS = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';

    public function __construct(
        private readonly EntityManagerInterface $em,
    ) {
    }

    public function getById(string $id): ?GameSession
    {
        return $this->em->find(GameSession::class, $id);
    }

    public function getByCode(string $code): ?GameSession
    {
        return $this->em->getRepository(GameSession::class)->findOneBy(
            ['code' => strtoupper($code)]
        );
    }

    public function getByHostId(string $hostId): array
    {
        return $this->em->getRepository(GameSession::class)->findBy(
            ['hostId' => $hostId],
            ['createdAt' => 'DESC']
        );
    }

    public function getByQuizId(string $quizId): array
    {
        return $this->em->getRepository(GameSession::class)->findBy(
            ['quizId' => $quizId],
            ['createdAt' => 'DESC']
        );
    }

    public function add(GameSession $session): void
    {
        if ($session->getCode() === '') {
            $session->setCode($this->generateUniqueCode());
        }
        $this->em->persist($session);
        $this->em->flush();
    }

    public function update(GameSession $session): void
    {
        $this->em->flush();
    }

    public function remove(GameSession $session): void
    {
        $this->em->remove($session);
        $this->em->flush();
    }

    private function generateUniqueCode(): string
    {
        do {
            $code = '';
            for ($i = 0; $i < 6; $i++) {
                $code .= self::CODE_CHARS[random_int(0, strlen(self::CODE_CHARS) - 1)];
            }
            $code = strtoupper($code);
        } while ($this->getByCode($code) !== null);
        return $code;
    }
}
