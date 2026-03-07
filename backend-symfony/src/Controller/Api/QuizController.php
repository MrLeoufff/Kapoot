<?php

declare(strict_types=1);

namespace App\Controller\Api;

use App\Application\Service\QuizService;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;

#[Route('/api/quizzes')]
final class QuizController extends AbstractController
{
    use \App\Controller\JsonResponseTrait;

    public function __construct(
        private readonly QuizService $quizService,
    ) {
    }

    #[Route('', name: 'api_quizzes_list', methods: ['GET'])]
    public function list(Request $request): JsonResponse
    {
        $ownerId = $request->query->get('ownerId');
        $ownerId = $ownerId !== null && $ownerId !== '' ? (string) $ownerId : null;
        $list = $this->quizService->listByOwner($ownerId);
        return $this->json($list);
    }

    #[Route('/published', name: 'api_quizzes_published', methods: ['GET'])]
    public function published(): JsonResponse
    {
        return $this->json($this->quizService->listPublished());
    }

    #[Route('/top10', name: 'api_quizzes_top10', methods: ['GET'])]
    public function top10(): JsonResponse
    {
        return $this->json($this->quizService->listTop10());
    }

    #[Route('/{id}', name: 'api_quizzes_get', requirements: ['id' => '[0-9a-fA-F\-]{36}'], methods: ['GET'])]
    public function get(string $id): JsonResponse
    {
        $quiz = $this->quizService->getById($id);
        if ($quiz === null) {
            return $this->json(['error' => 'Quiz introuvable.'], Response::HTTP_NOT_FOUND);
        }
        return $this->json($quiz);
    }

    #[Route('/{id}/detail', name: 'api_quizzes_detail', requirements: ['id' => '[0-9a-fA-F\-]{36}'], methods: ['GET'])]
    public function detail(string $id): JsonResponse
    {
        $detail = $this->quizService->getDetail($id);
        if ($detail === null) {
            return $this->json(['error' => 'Quiz introuvable.'], Response::HTTP_NOT_FOUND);
        }
        return $this->json($detail);
    }

    #[Route('', name: 'api_quizzes_create', methods: ['POST'])]
    public function create(Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $title = (string) ($data['title'] ?? '');
        $description = (string) ($data['description'] ?? '');
        $ownerId = (string) ($data['ownerId'] ?? '');

        try {
            $quiz = $this->quizService->create($title, $description, $ownerId);
            return $this->json($quiz, Response::HTTP_CREATED);
        } catch (\InvalidArgumentException|\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        }
    }

    #[Route('/{id}', name: 'api_quizzes_update', requirements: ['id' => '[0-9a-fA-F\-]{36}'], methods: ['PUT'])]
    public function update(string $id, Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $title = (string) ($data['title'] ?? '');
        $description = (string) ($data['description'] ?? '');

        try {
            $this->quizService->update($id, $title, $description);
            return new JsonResponse(null, Response::HTTP_NO_CONTENT);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_NOT_FOUND);
        }
    }

    #[Route('/{id}/publish', name: 'api_quizzes_publish', requirements: ['id' => '[0-9a-fA-F\-]{36}'], methods: ['POST'])]
    public function publish(string $id): JsonResponse
    {
        try {
            $this->quizService->publish($id);
            return new JsonResponse(null, Response::HTTP_NO_CONTENT);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_NOT_FOUND);
        }
    }
}
