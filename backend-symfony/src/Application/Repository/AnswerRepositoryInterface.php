<?php

declare(strict_types=1);

namespace App\Application\Repository;

use App\Entity\Answer;

interface AnswerRepositoryInterface
{
    public function countCorrectForQuestionInSession(string $sessionId, string $questionId): int;

    /** @param list<string> $playerIds */
    public function deleteByPlayerIds(array $playerIds): void;

    public function add(Answer $answer): void;
}
