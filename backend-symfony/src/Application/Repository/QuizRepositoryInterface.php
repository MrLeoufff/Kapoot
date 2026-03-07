<?php

declare(strict_types=1);

namespace App\Application\Repository;

use App\Entity\Quiz;

interface QuizRepositoryInterface
{
    public function getById(string $id): ?Quiz;

    /** @return list<Quiz> */
    public function getAll(): array;

    /** @return list<Quiz> */
    public function getByOwnerId(string $ownerId): array;

    /** @return list<Quiz> */
    public function getPublished(): array;

    /** @return list<Quiz> */
    public function getTopPlayed(int $count): array;

    public function add(Quiz $quiz): void;

    public function update(Quiz $quiz): void;

    public function remove(Quiz $quiz): void;
}
