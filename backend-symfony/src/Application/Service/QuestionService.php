<?php

declare(strict_types=1);

namespace App\Application\Service;

use App\Application\Repository\ChoiceRepositoryInterface;
use App\Application\Repository\QuestionRepositoryInterface;
use App\Application\Repository\QuizRepositoryInterface;
use App\Entity\Choice;
use App\Entity\Question;
use App\Enum\QuestionType;

final class QuestionService
{
    public function __construct(
        private readonly QuizRepositoryInterface $quizRepository,
        private readonly QuestionRepositoryInterface $questionRepository,
        private readonly ChoiceRepositoryInterface $choiceRepository,
    ) {
    }

    /**
     * @param list<array{text: string, isCorrect: bool, order: int}> $choices
     * @return array{id: string, text: string, type: int, explanation: string|null, order: int, choices: list<array>}
     */
    public function add(string $quizId, string $text, int $type, ?string $explanation, int $order, array $choices): array
    {
        $quiz = $this->quizRepository->getById($quizId);
        if ($quiz === null) {
            throw new \RuntimeException('Quiz introuvable.');
        }
        if (trim($text) === '') {
            throw new \InvalidArgumentException('Le texte de la question est obligatoire.');
        }
        if (count($choices) < 2) {
            throw new \InvalidArgumentException('Au moins deux choix sont requis.');
        }

        $question = new Question();
        $question->setQuizId($quizId);
        $question->setText(trim($text));
        $question->setType(QuestionType::from($type));
        $question->setExplanation($explanation !== null && $explanation !== '' ? trim($explanation) : null);
        $question->setOrder($order);
        $this->questionRepository->add($question);

        $sortOrder = 0;
        $choiceArrays = [];
        usort($choices, fn ($a, $b) => ($a['order'] ?? 0) <=> ($b['order'] ?? 0));
        foreach ($choices as $c) {
            $choice = new Choice();
            $choice->setQuestionId($question->getId());
            $choice->setText(trim((string) ($c['text'] ?? '')));
            $choice->setIsCorrect((bool) ($c['isCorrect'] ?? false));
            $choice->setOrder($sortOrder++);
            $this->choiceRepository->add($choice);
            $choiceArrays[] = ['id' => $choice->getId(), 'text' => $choice->getText(), 'isCorrect' => $choice->isCorrect(), 'order' => $choice->getOrder()];
        }

        return [
            'id' => $question->getId(),
            'text' => $question->getText(),
            'type' => $question->getType()->value,
            'explanation' => $question->getExplanation(),
            'order' => $question->getOrder(),
            'choices' => $choiceArrays,
        ];
    }

    /**
     * @param list<array{text: string, isCorrect: bool, order: int}> $choices
     */
    public function update(string $questionId, string $text, ?string $explanation, int $order, array $choices): void
    {
        $question = $this->questionRepository->getById($questionId);
        if ($question === null) {
            throw new \RuntimeException('Question introuvable.');
        }
        if (trim($text) === '') {
            throw new \InvalidArgumentException('Le texte de la question est obligatoire.');
        }

        $question->setText(trim($text));
        $question->setExplanation($explanation !== null && $explanation !== '' ? trim($explanation) : null);
        $question->setOrder($order);
        $this->questionRepository->update($question);

        $existingChoices = $this->choiceRepository->getByQuestionId($question->getId());
        foreach ($existingChoices as $c) {
            $this->choiceRepository->remove($c->getId());
        }

        $sortOrder = 0;
        usort($choices, fn ($a, $b) => ($a['order'] ?? 0) <=> ($b['order'] ?? 0));
        foreach ($choices as $c) {
            $choice = new Choice();
            $choice->setQuestionId($question->getId());
            $choice->setText(trim((string) ($c['text'] ?? '')));
            $choice->setIsCorrect((bool) ($c['isCorrect'] ?? false));
            $choice->setOrder($sortOrder++);
            $this->choiceRepository->add($choice);
        }
    }

    public function delete(string $questionId): void
    {
        $question = $this->questionRepository->getById($questionId);
        if ($question === null) {
            return;
        }
        $choices = $this->choiceRepository->getByQuestionId($questionId);
        foreach ($choices as $c) {
            $this->choiceRepository->remove($c->getId());
        }
        $this->questionRepository->remove($questionId);
    }
}
