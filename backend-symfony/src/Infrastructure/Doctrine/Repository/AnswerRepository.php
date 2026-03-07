<?php

declare(strict_types=1);

namespace App\Infrastructure\Doctrine\Repository;

use App\Application\Repository\AnswerRepositoryInterface;
use App\Entity\Answer;
use Doctrine\ORM\EntityManagerInterface;

final class AnswerRepository implements AnswerRepositoryInterface
{
    public function __construct(
        private readonly EntityManagerInterface $em,
    ) {
    }

    public function countCorrectForQuestionInSession(string $sessionId, string $questionId): int
    {
        $conn = $this->em->getConnection();
        $sql = 'SELECT COUNT(a.id) FROM answers a INNER JOIN players p ON p.id = a.player_id WHERE a.question_id = :qid AND p.game_session_id = :sid AND a.is_correct = 1';
        $result = $conn->executeQuery($sql, ['qid' => $questionId, 'sid' => $sessionId]);
        return (int) $result->fetchOne();
    }

    public function deleteByPlayerIds(array $playerIds): void
    {
        if ($playerIds === []) {
            return;
        }
        $this->em->createQueryBuilder()
            ->delete(Answer::class, 'a')
            ->where('a.playerId IN (:ids)')
            ->setParameter('ids', $playerIds)
            ->getQuery()
            ->execute();
    }

    public function add(Answer $answer): void
    {
        $this->em->persist($answer);
        $this->em->flush();
    }
}
