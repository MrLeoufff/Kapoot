<?php

declare(strict_types=1);

namespace App\Application\Service;

use App\Application\Repository\UserRepositoryInterface;
use App\Entity\User;
use Symfony\Component\PasswordHasher\Hasher\UserPasswordHasherInterface;

final class AuthService
{
    public function __construct(
        private readonly UserRepositoryInterface $userRepository,
        private readonly UserPasswordHasherInterface $passwordHasher,
        private readonly string $initialAdminEmail = '',
    ) {
    }

    /**
     * @return array{id: string, email: string, pseudo: string, isAdmin: bool}
     * @throws \InvalidArgumentException
     */
    public function register(string $email, string $password, string $pseudo): array
    {
        $email = trim($email);
        $pseudo = trim($pseudo);

        if ($email === '' || $password === '' || $pseudo === '') {
            throw new \InvalidArgumentException('Email, password et pseudo requis.');
        }

        if ($this->userRepository->getByEmail($email) !== null) {
            throw new \RuntimeException('Un compte avec cet email existe déjà.');
        }

        $user = new User();
        $user->setEmail($email);
        $user->setPasswordHash($this->passwordHasher->hashPassword($user, $password));
        $user->setPseudo($pseudo);
        $initialAdmin = $this->initialAdminEmail !== ''
            && strtolower(trim($this->initialAdminEmail)) === strtolower($email);
        if ($initialAdmin) {
            $user->setIsAdmin(true);
        }
        $this->userRepository->add($user);

        return [
            'id' => $user->getId(),
            'email' => $user->getEmail(),
            'pseudo' => $user->getPseudo(),
            'isAdmin' => $user->isAdmin(),
        ];
    }
}
