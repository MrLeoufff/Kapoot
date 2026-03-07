<?php

declare(strict_types=1);

namespace App\Application\Repository;

use App\Entity\User;

interface UserRepositoryInterface
{
    public function getById(string $id): ?User;

    public function getByEmail(string $email): ?User;

    /** @return list<User> */
    public function getAll(): array;

    public function add(User $user): void;

    public function update(User $user): void;

    public function remove(User $user): void;
}
