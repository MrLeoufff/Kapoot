<?php

declare(strict_types=1);

namespace App\Application\Service;

use App\Application\Repository\AnswerRepositoryInterface;
use App\Application\Repository\ChoiceRepositoryInterface;
use App\Application\Repository\GameSessionRepositoryInterface;
use App\Application\Repository\PlayerRepositoryInterface;
use App\Application\Repository\QuestionRepositoryInterface;
use App\Application\Repository\QuizRepositoryInterface;
use App\Application\Repository\UserRepositoryInterface;

final class AdminService
{
    public function __construct(
        private readonly UserRepositoryInterface $userRepository,
        private readonly QuizRepositoryInterface $quizRepository,
        private readonly GameSessionRepositoryInterface $gameSessionRepository,
        private readonly PlayerRepositoryInterface $playerRepository,
        private readonly AnswerRepositoryInterface $answerRepository,
        private readonly QuestionRepositoryInterface $questionRepository,
        private readonly ChoiceRepositoryInterface $choiceRepository,
    ) {
    }

    /** @return list<array{id: string, email: string, pseudo: string, isAdmin: bool, dateCreated: string}> */
    public function listUsers(): array
    {
        $users = $this->userRepository->getAll();
        $result = [];
        foreach ($users as $u) {
            $result[] = [
                'id' => $u->getId(),
                'email' => $u->getEmail(),
                'pseudo' => $u->getPseudo(),
                'isAdmin' => $u->isAdmin(),
                'dateCreated' => $u->getDateCreated()->format(\DateTimeInterface::ATOM),
            ];
        }
        return $result;
    }

    /** @return list<array> */
    public function listQuizzes(): array
    {
        $quizzes = $this->quizRepository->getAll();
        $result = [];
        foreach ($quizzes as $q) {
            $result[] = [
                'id' => $q->getId(),
                'title' => $q->getTitle(),
                'description' => $q->getDescription(),
                'status' => $q->getStatus()->value,
                'ownerId' => $q->getOwnerId(),
                'createdAt' => $q->getCreatedAt()->format(\DateTimeInterface::ATOM),
                'updatedAt' => $q->getUpdatedAt()->format(\DateTimeInterface::ATOM),
            ];
        }
        return $result;
    }

    public function deleteUser(string $userId): void
    {
        $user = $this->userRepository->getById($userId);
        if ($user === null) {
            throw new \RuntimeException('Utilisateur introuvable.');
        }

        $quizzes = $this->quizRepository->getByOwnerId($userId);
        foreach ($quizzes as $quiz) {
            $this->deleteQuiz($quiz->getId());
        }

        $sessions = $this->gameSessionRepository->getByHostId($userId);
        foreach ($sessions as $session) {
            $this->gameSessionRepository->remove($session);
        }

        $this->userRepository->remove($user);
    }

    public function setUserAdmin(string $userId, bool $isAdmin): void
    {
        $user = $this->userRepository->getById($userId);
        if ($user === null) {
            throw new \RuntimeException('Utilisateur introuvable.');
        }
        $user->setIsAdmin($isAdmin);
        $this->userRepository->update($user);
    }

    public function deleteQuiz(string $quizId): void
    {
        $quiz = $this->quizRepository->getById($quizId);
        if ($quiz === null) {
            throw new \RuntimeException('Quiz introuvable.');
        }

        $sessions = $this->gameSessionRepository->getByQuizId($quizId);
        $playerIds = [];
        foreach ($sessions as $session) {
            $players = $this->playerRepository->getByGameSessionId($session->getId());
            foreach ($players as $p) {
                $playerIds[] = $p->getId();
            }
        }
        $this->answerRepository->deleteByPlayerIds($playerIds);

        foreach ($sessions as $session) {
            $this->gameSessionRepository->remove($session);
        }

        $questions = $this->questionRepository->getByQuizId($quizId);
        foreach ($questions as $q) {
            foreach ($this->choiceRepository->getByQuestionId($q->getId()) as $c) {
                $this->choiceRepository->remove($c->getId());
            }
            $this->questionRepository->remove($q->getId());
        }
        $this->quizRepository->remove($quiz);
    }
}
