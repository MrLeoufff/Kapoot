<?php

declare(strict_types=1);

namespace App\Application\Repository;

use App\Entity\Question;

interface QuestionRepositoryInterface
{
    public function getById(string $id): ?Question;

    /** @return list<Question> */
    public function getByQuizId(string $quizId): array;

    public function add(Question $question): void;

    public function update(Question $question): void;

    public function remove(string $id): void;
}
