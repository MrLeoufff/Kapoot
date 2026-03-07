<?php

declare(strict_types=1);

namespace App\Infrastructure\Doctrine\Repository;

use App\Application\Repository\ChoiceRepositoryInterface;
use App\Entity\Choice;
use Doctrine\ORM\EntityManagerInterface;

final class ChoiceRepository implements ChoiceRepositoryInterface
{
    public function __construct(
        private readonly EntityManagerInterface $em,
    ) {
    }

    public function getById(string $id): ?Choice
    {
        return $this->em->find(Choice::class, $id);
    }

    public function getByQuestionId(string $questionId): array
    {
        return $this->em->getRepository(Choice::class)->findBy(
            ['questionId' => $questionId],
            ['order' => 'ASC']
        );
    }

    public function add(Choice $choice): void
    {
        $this->em->persist($choice);
        $this->em->flush();
    }

    public function remove(string $id): void
    {
        $choice = $this->em->find(Choice::class, $id);
        if ($choice !== null) {
            $this->em->remove($choice);
            $this->em->flush();
        }
    }
}
