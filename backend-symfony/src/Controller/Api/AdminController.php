<?php

declare(strict_types=1);

namespace App\Controller\Api;

use App\Application\Service\AdminService;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;
use Symfony\Component\Security\Http\Attribute\IsGranted;

#[Route('/api/admin')]
#[IsGranted('ROLE_ADMIN')]
final class AdminController extends AbstractController
{
    use \App\Controller\JsonResponseTrait;

    public function __construct(
        private readonly AdminService $adminService,
    ) {
    }

    #[Route('/users', name: 'api_admin_users', methods: ['GET'])]
    public function users(): JsonResponse
    {
        return $this->json($this->adminService->listUsers());
    }

    #[Route('/quizzes', name: 'api_admin_quizzes', methods: ['GET'])]
    public function quizzes(): JsonResponse
    {
        return $this->json($this->adminService->listQuizzes());
    }

    #[Route('/users/{id}', name: 'api_admin_users_delete', requirements: ['id' => '[0-9a-fA-F\-]{36}'], methods: ['DELETE'])]
    public function deleteUser(string $id): JsonResponse
    {
        try {
            $this->adminService->deleteUser($id);
            return new JsonResponse(null, Response::HTTP_NO_CONTENT);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_NOT_FOUND);
        }
    }

    #[Route('/users/{id}/admin', name: 'api_admin_users_set_admin', requirements: ['id' => '[0-9a-fA-F\-]{36}'], methods: ['PUT'])]
    public function setUserAdmin(string $id, Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $isAdmin = (bool) ($data['isAdmin'] ?? false);

        try {
            $this->adminService->setUserAdmin($id, $isAdmin);
            return new JsonResponse(null, Response::HTTP_NO_CONTENT);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_NOT_FOUND);
        }
    }

    #[Route('/quizzes/{id}', name: 'api_admin_quizzes_delete', requirements: ['id' => '[0-9a-fA-F\-]{36}'], methods: ['DELETE'])]
    public function deleteQuiz(string $id): JsonResponse
    {
        try {
            $this->adminService->deleteQuiz($id);
            return new JsonResponse(null, Response::HTTP_NO_CONTENT);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_NOT_FOUND);
        }
    }
}
