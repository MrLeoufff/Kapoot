<?php

declare(strict_types=1);

namespace App\Entity;

use App\Enum\GameSessionStatus;
use Doctrine\DBAL\Types\Types;
use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'game_sessions')]
#[ORM\UniqueConstraint(name: 'uniq_code', columns: ['code'])]
class GameSession
{
    #[ORM\Id]
    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $id;

    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $quizId;

    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $hostId;

    #[ORM\Column(type: Types::STRING, length: 20)]
    private string $code;

    #[ORM\Column(type: Types::INTEGER, enumType: GameSessionStatus::class)]
    private GameSessionStatus $status;

    #[ORM\Column(type: Types::DATETIME_IMMUTABLE)]
    private \DateTimeImmutable $createdAt;

    #[ORM\Column(type: Types::DATETIME_IMMUTABLE, nullable: true)]
    private ?\DateTimeImmutable $startedAt = null;

    #[ORM\Column(type: Types::DATETIME_IMMUTABLE, nullable: true)]
    private ?\DateTimeImmutable $finishedAt = null;

    public function __construct()
    {
        $this->id = $this->generateUuid();
        $this->status = GameSessionStatus::Waiting;
        $this->createdAt = new \DateTimeImmutable();
        $this->code = '';
    }

    public function getId(): string
    {
        return $this->id;
    }

    public function getQuizId(): string
    {
        return $this->quizId;
    }

    public function setQuizId(string $quizId): self
    {
        $this->quizId = $quizId;
        return $this;
    }

    public function getHostId(): string
    {
        return $this->hostId;
    }

    public function setHostId(string $hostId): self
    {
        $this->hostId = $hostId;
        return $this;
    }

    public function getCode(): string
    {
        return $this->code;
    }

    public function setCode(string $code): self
    {
        $this->code = $code;
        return $this;
    }

    public function getStatus(): GameSessionStatus
    {
        return $this->status;
    }

    public function setStatus(GameSessionStatus $status): self
    {
        $this->status = $status;
        return $this;
    }

    public function getCreatedAt(): \DateTimeImmutable
    {
        return $this->createdAt;
    }

    public function getStartedAt(): ?\DateTimeImmutable
    {
        return $this->startedAt;
    }

    public function setStartedAt(?\DateTimeImmutable $startedAt): self
    {
        $this->startedAt = $startedAt;
        return $this;
    }

    public function getFinishedAt(): ?\DateTimeImmutable
    {
        return $this->finishedAt;
    }

    public function setFinishedAt(?\DateTimeImmutable $finishedAt): self
    {
        $this->finishedAt = $finishedAt;
        return $this;
    }

    private static function generateUuid(): string
    {
        $data = random_bytes(16);
        $data[6] = chr(ord($data[6]) & 0x0f | 0x40);
        $data[8] = chr(ord($data[8]) & 0x3f | 0x80);
        return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
    }

}
