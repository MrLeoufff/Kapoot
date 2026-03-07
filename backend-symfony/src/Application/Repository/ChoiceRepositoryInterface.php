<?php

declare(strict_types=1);

namespace App\Application\Repository;

use App\Entity\Choice;

interface ChoiceRepositoryInterface
{
    public function getById(string $id): ?Choice;

    /** @return list<Choice> */
    public function getByQuestionId(string $questionId): array;

    public function add(Choice $choice): void;

    public function remove(string $id): void;
}
