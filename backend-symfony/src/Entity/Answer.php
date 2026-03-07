<?php

declare(strict_types=1);

namespace App\Entity;

use Doctrine\DBAL\Types\Types;
use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'answers')]
class Answer
{
    #[ORM\Id]
    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $id;

    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $playerId;

    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $questionId;

    /** @var list<string> IDs des choix sélectionnés (JSON) */
    #[ORM\Column(type: Types::JSON)]
    private array $selectedChoiceIds = [];

    #[ORM\Column(type: Types::BOOLEAN)]
    private bool $isCorrect = false;

    #[ORM\Column(type: Types::DATETIME_IMMUTABLE)]
    private \DateTimeImmutable $answeredAt;

    public function __construct()
    {
        $this->id = $this->generateUuid();
        $this->answeredAt = new \DateTimeImmutable();
    }

    public function getId(): string
    {
        return $this->id;
    }

    public function getPlayerId(): string
    {
        return $this->playerId;
    }

    public function setPlayerId(string $playerId): self
    {
        $this->playerId = $playerId;
        return $this;
    }

    public function getQuestionId(): string
    {
        return $this->questionId;
    }

    public function setQuestionId(string $questionId): self
    {
        $this->questionId = $questionId;
        return $this;
    }

    /** @return list<string> */
    public function getSelectedChoiceIds(): array
    {
        return $this->selectedChoiceIds;
    }

    /** @param list<string> $selectedChoiceIds */
    public function setSelectedChoiceIds(array $selectedChoiceIds): self
    {
        $this->selectedChoiceIds = $selectedChoiceIds;
        return $this;
    }

    public function isCorrect(): bool
    {
        return $this->isCorrect;
    }

    public function setIsCorrect(bool $isCorrect): self
    {
        $this->isCorrect = $isCorrect;
        return $this;
    }

    public function getAnsweredAt(): \DateTimeImmutable
    {
        return $this->answeredAt;
    }

    private static function generateUuid(): string
    {
        $data = random_bytes(16);
        $data[6] = chr(ord($data[6]) & 0x0f | 0x40);
        $data[8] = chr(ord($data[8]) & 0x3f | 0x80);
        return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
    }
}
