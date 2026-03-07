<?php

declare(strict_types=1);

namespace App\Infrastructure\Doctrine\Repository;

use App\Application\Repository\QuestionRepositoryInterface;
use App\Entity\Question;
use Doctrine\ORM\EntityManagerInterface;

final class QuestionRepository implements QuestionRepositoryInterface
{
    public function __construct(
        private readonly EntityManagerInterface $em,
    ) {
    }

    public function getById(string $id): ?Question
    {
        return $this->em->find(Question::class, $id);
    }

    public function getByQuizId(string $quizId): array
    {
        return $this->em->getRepository(Question::class)->findBy(
            ['quizId' => $quizId],
            ['order' => 'ASC']
        );
    }

    public function add(Question $question): void
    {
        $this->em->persist($question);
        $this->em->flush();
    }

    public function update(Question $question): void
    {
        $this->em->flush();
    }

    public function remove(string $id): void
    {
        $question = $this->em->find(Question::class, $id);
        if ($question !== null) {
            $this->em->remove($question);
            $this->em->flush();
        }
    }
}
