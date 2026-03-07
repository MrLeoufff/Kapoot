<?php

declare(strict_types=1);

namespace App\Entity;

use Doctrine\DBAL\Types\Types;
use Doctrine\ORM\Mapping as ORM;

#[ORM\Entity]
#[ORM\Table(name: 'players')]
class Player
{
    #[ORM\Id]
    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $id;

    #[ORM\Column(type: Types::STRING, length: 36, nullable: true)]
    private ?string $userId = null;

    #[ORM\Column(type: Types::STRING, length: 36)]
    private string $gameSessionId;

    #[ORM\Column(type: Types::STRING, length: 100)]
    private string $pseudo;

    #[ORM\Column(type: Types::BOOLEAN, options: ['default' => false])]
    private bool $hasLeft = false;

    public function __construct()
    {
        $this->id = $this->generateUuid();
    }

    public function getId(): string
    {
        return $this->id;
    }

    public function getUserId(): ?string
    {
        return $this->userId;
    }

    public function setUserId(?string $userId): self
    {
        $this->userId = $userId;
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

    public function getPseudo(): string
    {
        return $this->pseudo;
    }

    public function setPseudo(string $pseudo): self
    {
        $this->pseudo = $pseudo;
        return $this;
    }

    public function hasLeft(): bool
    {
        return $this->hasLeft;
    }

    public function setHasLeft(bool $hasLeft): self
    {
        $this->hasLeft = $hasLeft;
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
