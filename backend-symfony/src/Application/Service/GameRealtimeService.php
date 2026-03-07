<?php

declare(strict_types=1);

namespace App\Application\Service;

use App\Application\Repository\AnswerRepositoryInterface;
use App\Application\Repository\ChoiceRepositoryInterface;
use App\Application\Repository\GameSessionRepositoryInterface;
use App\Application\Repository\PlayerRepositoryInterface;
use App\Application\Repository\QuestionRepositoryInterface;
use App\Application\Repository\ScoreRepositoryInterface;
use App\Entity\Answer;
use App\Entity\Score;
use App\Enum\GameSessionStatus;
use Symfony\Component\Mercure\HubInterface;
use Symfony\Component\Mercure\Update;

final class GameRealtimeService
{
    private const TOPIC_PREFIX = 'session/';

    public function __construct(
        private readonly GameSessionRepositoryInterface $gameSessionRepository,
        private readonly PlayerRepositoryInterface $playerRepository,
        private readonly QuizService $quizService,
        private readonly QuestionRepositoryInterface $questionRepository,
        private readonly ChoiceRepositoryInterface $choiceRepository,
        private readonly AnswerRepositoryInterface $answerRepository,
        private readonly ScoreRepositoryInterface $scoreRepository,
        private readonly HubInterface $mercureHub,
    ) {
    }

    public function startGame(string $sessionId, string $hostUserId): void
    {
        $session = $this->gameSessionRepository->getById($sessionId);
        if ($session === null) {
            throw new \RuntimeException('Partie introuvable.');
        }
        if ($session->getHostId() !== $hostUserId) {
            throw new \RuntimeException('Non autorisé.');
        }
        if ($session->getStatus() !== GameSessionStatus::Waiting) {
            throw new \RuntimeException('Partie non disponible.');
        }
        $session->setStatus(GameSessionStatus::Running);
        $session->setStartedAt(new \DateTimeImmutable());
        $this->gameSessionRepository->update($session);

        $this->publish($sessionId, 'GameStarted', [
            'sessionId' => $sessionId,
            'quizId' => $session->getQuizId(),
        ]);
    }

    public function showQuestion(string $sessionId, string $hostUserId, int $questionIndex): void
    {
        $session = $this->gameSessionRepository->getById($sessionId);
        if ($session === null || $session->getHostId() !== $hostUserId) {
            throw new \RuntimeException('Partie introuvable ou non autorisé.');
        }
        $detail = $this->quizService->getDetail($session->getQuizId());
        if ($detail === null || !isset($detail['questions'][$questionIndex])) {
            throw new \RuntimeException('Question invalide.');
        }
        $q = $detail['questions'][$questionIndex];
        $choicesForClient = array_map(fn (array $c) => [
            'id' => $c['id'],
            'text' => $c['text'],
            'order' => $c['order'],
        ], $q['choices']);
        $allowMultiple = count(array_filter($q['choices'], fn ($c) => $c['isCorrect'] ?? false)) > 1;

        $this->publish($sessionId, 'ShowQuestion', [
            'questionId' => $q['id'],
            'text' => $q['text'],
            'type' => $q['type'],
            'order' => $q['order'],
            'choices' => $choicesForClient,
            'allowMultiple' => $allowMultiple,
        ]);
    }

    /** @param list<string> $choiceIds */
    public function submitAnswer(string $sessionId, string $playerId, string $questionId, array $choiceIds): void
    {
        $session = $this->gameSessionRepository->getById($sessionId);
        $player = $this->playerRepository->getById($playerId);
        if ($session === null || $player === null || $player->getGameSessionId() !== $sessionId) {
            throw new \RuntimeException('Partie ou joueur invalide.');
        }
        $question = $this->questionRepository->getById($questionId);
        if ($question === null) {
            throw new \RuntimeException('Question introuvable.');
        }
        $correctChoices = array_map(
            fn ($c) => $c->getId(),
            array_filter($this->choiceRepository->getByQuestionId($questionId), fn ($c) => $c->isCorrect())
        );
        sort($correctChoices);
        $submitted = $choiceIds;
        sort($submitted);
        $isCorrect = $correctChoices === $submitted;

        $existingCorrect = $this->answerRepository->countCorrectForQuestionInSession($sessionId, $questionId);
        $rank = $isCorrect ? $existingCorrect + 1 : 0;
        $pointsEarned = $isCorrect ? $this->getPointsForRank($rank) : 0;

        $answer = new Answer();
        $answer->setPlayerId($playerId);
        $answer->setQuestionId($questionId);
        $answer->setSelectedChoiceIds(array_values($choiceIds));
        $answer->setIsCorrect($isCorrect);
        $this->answerRepository->add($answer);

        $this->publish($sessionId, 'PlayerAnswered', ['playerId' => $playerId, 'pseudo' => $player->getPseudo()]);
        $this->publish($sessionId, 'PointsEarned', [
            'playerId' => $playerId,
            'pseudo' => $player->getPseudo(),
            'pointsEarned' => $pointsEarned,
            'rank' => $rank,
        ]);

        $score = $this->scoreRepository->getByPlayerAndGameSession($playerId, $sessionId);
        if ($score === null) {
            $score = new Score();
            $score->setPlayerId($playerId);
            $score->setGameSessionId($sessionId);
            $score->setTotalPoints($pointsEarned);
            $this->scoreRepository->add($score);
        } else {
            $score->setTotalPoints($score->getTotalPoints() + $pointsEarned);
            $this->scoreRepository->update($score);
        }

        $this->recomputeRanksAndBroadcast($sessionId);
    }

    public function endQuestion(string $sessionId, string $hostUserId, string $questionId, ?string $explanation): void
    {
        $session = $this->gameSessionRepository->getById($sessionId);
        if ($session === null || $session->getHostId() !== $hostUserId) {
            throw new \RuntimeException('Non autorisé.');
        }
        $this->publish($sessionId, 'ShowResult', ['questionId' => $questionId, 'explanation' => $explanation]);
        $this->recomputeRanksAndBroadcast($sessionId);
    }

    public function endGame(string $sessionId, string $hostUserId): void
    {
        $session = $this->gameSessionRepository->getById($sessionId);
        if ($session === null || $session->getHostId() !== $hostUserId) {
            throw new \RuntimeException('Non autorisé.');
        }
        $session->setStatus(GameSessionStatus::Finished);
        $session->setFinishedAt(new \DateTimeImmutable());
        $this->gameSessionRepository->update($session);

        $scores = $this->scoreRepository->getByGameSessionId($sessionId);
        $players = $this->playerRepository->getByGameSessionId($sessionId);
        $playerMap = [];
        foreach ($players as $p) {
            $playerMap[$p->getId()] = $p->getPseudo();
        }
        $ranking = [];
        $idx = 1;
        foreach ($scores as $s) {
            $ranking[] = [
                'playerId' => $s->getPlayerId(),
                'totalPoints' => $s->getTotalPoints(),
                'rank' => $idx,
                'pseudo' => $playerMap[$s->getPlayerId()] ?? '',
            ];
            $idx++;
        }
        $this->publish($sessionId, 'GameEnded', ['ranking' => $ranking]);
    }

    public function setPlayerHasLeft(string $playerId, bool $hasLeft): void
    {
        $player = $this->playerRepository->getById($playerId);
        if ($player !== null) {
            $player->setHasLeft($hasLeft);
            $this->playerRepository->update($player);
        }
    }

    private function recomputeRanksAndBroadcast(string $sessionId): void
    {
        $scores = $this->scoreRepository->getByGameSessionId($sessionId);
        $rank = 1;
        foreach ($scores as $s) {
            $s->setRank($rank);
            $this->scoreRepository->update($s);
            $rank++;
        }
        $players = $this->playerRepository->getByGameSessionId($sessionId);
        $playerMap = [];
        foreach ($players as $p) {
            $playerMap[$p->getId()] = $p->getPseudo();
        }
        $ranking = [];
        foreach ($scores as $s) {
            $ranking[] = [
                'playerId' => $s->getPlayerId(),
                'totalPoints' => $s->getTotalPoints(),
                'rank' => $s->getRank(),
                'pseudo' => $playerMap[$s->getPlayerId()] ?? '',
            ];
        }
        $this->publish($sessionId, 'Ranking', ['ranking' => $ranking]);
    }

    private function publish(string $sessionId, string $event, array $data): void
    {
        $payload = ['event' => $event] + $data;
        $this->mercureHub->publish(new Update(
            self::TOPIC_PREFIX . $sessionId,
            json_encode($payload, \JSON_THROW_ON_ERROR)
        ));
    }

    private function getPointsForRank(int $rank): int
    {
        return match ($rank) {
            1 => 100,
            2 => 80,
            3 => 60,
            4 => 50,
            default => 40,
        };
    }
}
