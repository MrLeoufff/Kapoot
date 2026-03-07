<?php

declare(strict_types=1);

namespace App\Infrastructure\Doctrine\Repository;

use App\Application\Repository\QuizRepositoryInterface;
use App\Entity\Quiz;
use App\Enum\QuizStatus;
use Doctrine\ORM\EntityManagerInterface;

final class QuizRepository implements QuizRepositoryInterface
{
    public function __construct(
        private readonly EntityManagerInterface $em,
    ) {
    }

    public function getById(string $id): ?Quiz
    {
        return $this->em->find(Quiz::class, $id);
    }

    public function getAll(): array
    {
        return $this->em->getRepository(Quiz::class)->findBy([], ['updatedAt' => 'DESC']);
    }

    public function getByOwnerId(string $ownerId): array
    {
        return $this->em->getRepository(Quiz::class)->findBy(
            ['ownerId' => $ownerId],
            ['updatedAt' => 'DESC']
        );
    }

    public function getPublished(): array
    {
        return $this->em->getRepository(Quiz::class)->findBy(
            ['status' => QuizStatus::Published],
            ['updatedAt' => 'DESC']
        );
    }

    public function getTopPlayed(int $count): array
    {
        // Simplification: retourne les N derniers quiz publiés (pas de comptage de parties pour l'instant)
        return array_slice($this->getPublished(), 0, $count);
    }

    public function add(Quiz $quiz): void
    {
        $this->em->persist($quiz);
        $this->em->flush();
    }

    public function update(Quiz $quiz): void
    {
        $this->em->flush();
    }

    public function remove(Quiz $quiz): void
    {
        $this->em->remove($quiz);
        $this->em->flush();
    }
}
