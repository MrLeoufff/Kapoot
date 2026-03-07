<?php

declare(strict_types=1);

namespace App\Controller\Api;

use App\Application\Service\UserService;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;

#[Route('/api/users')]
final class UserController extends AbstractController
{
    use \App\Controller\JsonResponseTrait;

    public function __construct(
        private readonly UserService $userService,
    ) {
    }

    #[Route('/{userId}/profile', name: 'api_users_profile', requirements: ['userId' => '[0-9a-fA-F\-]{36}'], methods: ['GET'])]
    public function profile(string $userId): JsonResponse
    {
        $profile = $this->userService->getProfile($userId);
        if ($profile === null) {
            return $this->json(['error' => 'Utilisateur introuvable.'], Response::HTTP_NOT_FOUND);
        }
        return $this->json($profile);
    }
}
