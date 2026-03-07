<?php

declare(strict_types=1);

namespace App\EventListener;

use App\Entity\User;
use Lexik\Bundle\JWTAuthenticationBundle\Event\AuthenticationSuccessEvent;

/**
 * Ajoute l'objet user à la réponse JSON du login JWT (token + user),
 * pour compatibilité avec le frontend qui attend { token, user }.
 */
final class JwtAuthenticationSuccessListener
{
    public function onAuthenticationSuccess(AuthenticationSuccessEvent $event): void
    {
        $data = $event->getData();
        $user = $event->getUser();

        if (!$user instanceof User) {
            return;
        }

        $data['user'] = [
            'id' => $user->getId(),
            'email' => $user->getEmail(),
            'pseudo' => $user->getPseudo(),
            'avatarUrl' => $user->getAvatarUrl(),
            'isAdmin' => $user->isAdmin(),
        ];

        $event->setData($data);
    }
}
