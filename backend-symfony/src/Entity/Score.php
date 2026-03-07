<?php

declare(strict_types=1);

namespace App\Entity;

use Doctrine\DBAL\Types\Types;
use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'scores')]
class Score
{
    #[ORM\Id]
    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $id;

    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $playerId;

    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $gameSessionId;

    #[ORM\Column(type: Types::INTEGER)]
    private int $totalPoints = 0;

    #[ORM\Column(type: Types::INTEGER, nullable: true, name: 'score_rank')]
    private ?int $rank = null;

    public function __construct()
    {
        $this->id = $this->generateUuid();
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

    public function getGameSessionId(): string
    {
        return $this->gameSessionId;
    }

    public function setGameSessionId(string $gameSessionId): self
    {
        $this->gameSessionId = $gameSessionId;
        return $this;
    }

    public function getTotalPoints(): int
    {
        return $this->totalPoints;
    }

    public function setTotalPoints(int $totalPoints): self
    {
        $this->totalPoints = $totalPoints;
        return $this;
    }

    public function getRank(): ?int
    {
        return $this->rank;
    }

    public function setRank(?int $rank): self
    {
        $this->rank = $rank;
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
