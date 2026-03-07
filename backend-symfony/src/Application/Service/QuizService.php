<?php

declare(strict_types=1);

namespace App\Application\Service;

use App\Application\Repository\ChoiceRepositoryInterface;
use App\Application\Repository\QuestionRepositoryInterface;
use App\Application\Repository\QuizRepositoryInterface;
use App\Application\Repository\UserRepositoryInterface;
use App\Entity\Choice;
use App\Entity\Quiz;
use App\Entity\Question;
use App\Enum\QuizStatus;

final class QuizService
{
    public function __construct(
        private readonly QuizRepositoryInterface $quizRepository,
        private readonly UserRepositoryInterface $userRepository,
        private readonly QuestionRepositoryInterface $questionRepository,
        private readonly ChoiceRepositoryInterface $choiceRepository,
    ) {
    }

    /**
     * @return array{id: string, title: string, description: string, status: int, ownerId: string, createdAt: string, updatedAt: string}
     */
    public function create(string $title, string $description, string $ownerId): array
    {
        if (trim($title) === '') {
            throw new \InvalidArgumentException('Le titre du quiz est obligatoire.');
        }

        if ($this->userRepository->getById($ownerId) === null) {
            throw new \RuntimeException('Le propriétaire du quiz n\'existe pas.');
        }

        $quiz = new Quiz();
        $quiz->setTitle(trim($title));
        $quiz->setDescription(trim($description));
        $quiz->setOwnerId($ownerId);
        $this->quizRepository->add($quiz);

        return $this->quizToArray($quiz);
    }

    public function update(string $id, string $title, string $description): void
    {
        $quiz = $this->quizRepository->getById($id);
        if ($quiz === null) {
            throw new \RuntimeException('Quiz introuvable.');
        }
        $quiz->setTitle(trim($title));
        $quiz->setDescription(trim($description));
        $this->quizRepository->update($quiz);
    }

    public function publish(string $id): void
    {
        $quiz = $this->quizRepository->getById($id);
        if ($quiz === null) {
            throw new \RuntimeException('Quiz introuvable.');
        }
        $quiz->setStatus(QuizStatus::Published);
        $this->quizRepository->update($quiz);
    }

    /**
     * @return array{id: string, title: string, description: string, status: int, ownerId: string, createdAt: string, updatedAt: string, questions: array}|null
     */
    public function getDetail(string $id): ?array
    {
        $quiz = $this->quizRepository->getById($id);
        if ($quiz === null) {
            return null;
        }

        $questions = $this->questionRepository->getByQuizId($quiz->getId());
        $questionArrays = [];
        foreach ($questions as $q) {
            $choices = $this->choiceRepository->getByQuestionId($q->getId());
            $choiceArrays = array_map(fn (Choice $c) => [
                'id' => $c->getId(),
                'text' => $c->getText(),
                'isCorrect' => $c->isCorrect(),
                'order' => $c->getOrder(),
            ], $choices);
            $questionArrays[] = [
                'id' => $q->getId(),
                'text' => $q->getText(),
                'type' => $q->getType()->value,
                'explanation' => $q->getExplanation(),
                'order' => $q->getOrder(),
                'choices' => $choiceArrays,
            ];
        }

        usort($questionArrays, fn ($a, $b) => $a['order'] <=> $b['order']);

        return [
            'id' => $quiz->getId(),
            'title' => $quiz->getTitle(),
            'description' => $quiz->getDescription(),
            'status' => $quiz->getStatus()->value,
            'ownerId' => $quiz->getOwnerId(),
            'questions' => $questionArrays,
        ];
    }

    /**
     * @return list<array{id: string, title: string, description: string, status: int, ownerId: string, createdAt: string, updatedAt: string}>
     */
    public function listByOwner(?string $ownerId): array
    {
        if ($ownerId !== null && $ownerId !== '') {
            $quizzes = $this->quizRepository->getByOwnerId($ownerId);
        } else {
            $quizzes = $this->quizRepository->getPublished();
        }
        return array_map(fn (Quiz $q) => $this->quizToArray($q), $quizzes);
    }

    /** @return list<array> */
    public function listPublished(): array
    {
        return array_map(fn (Quiz $q) => $this->quizToArray($q), $this->quizRepository->getPublished());
    }

    /** @return list<array> */
    public function listTop10(): array
    {
        return array_map(fn (Quiz $q) => $this->quizToArray($q), $this->quizRepository->getTopPlayed(10));
    }

    /**
     * @return array{id: string, title: string, description: string, status: int, ownerId: string, createdAt: string, updatedAt: string}|null
     */
    public function getById(string $id): ?array
    {
        $quiz = $this->quizRepository->getById($id);
        return $quiz === null ? null : $this->quizToArray($quiz);
    }

    private function quizToArray(Quiz $q): array
    {
        return [
            'id' => $q->getId(),
            'title' => $q->getTitle(),
            'description' => $q->getDescription(),
            'status' => $q->getStatus()->value,
            'ownerId' => $q->getOwnerId(),
            'createdAt' => $q->getCreatedAt()->format(\DateTimeInterface::ATOM),
            'updatedAt' => $q->getUpdatedAt()->format(\DateTimeInterface::ATOM),
        ];
    }
}
