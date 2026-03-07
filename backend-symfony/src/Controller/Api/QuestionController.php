<?php

declare(strict_types=1);

namespace App\Controller\Api;

use App\Application\Service\QuestionService;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;

#[Route('/api')]
final class QuestionController extends AbstractController
{
    use \App\Controller\JsonResponseTrait;

    public function __construct(
        private readonly QuestionService $questionService,
    ) {
    }

    #[Route('/quizzes/{quizId}/questions', name: 'api_questions_add', requirements: ['quizId' => '[0-9a-fA-F\-]{36}'], methods: ['POST'])]
    public function add(string $quizId, Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $text = (string) ($data['text'] ?? '');
        $type = (int) ($data['type'] ?? 0);
        $explanation = isset($data['explanation']) ? (string) $data['explanation'] : null;
        $order = (int) ($data['order'] ?? 0);
        $choices = $data['choices'] ?? [];
        if (!is_array($choices)) {
            $choices = [];
        }

        try {
            $question = $this->questionService->add($quizId, $text, $type, $explanation, $order, $choices);
            return $this->json($question, Response::HTTP_CREATED);
        } catch (\InvalidArgumentException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_NOT_FOUND);
        }
    }

    #[Route('/questions/{id}', name: 'api_questions_update', requirements: ['id' => '[0-9a-fA-F\-]{36}'], methods: ['PUT'])]
    public function update(string $id, Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $text = (string) ($data['text'] ?? '');
        $explanation = isset($data['explanation']) ? (string) $data['explanation'] : null;
        $order = (int) ($data['order'] ?? 0);
        $choices = $data['choices'] ?? [];
        if (!is_array($choices)) {
            $choices = [];
        }

        try {
            $this->questionService->update($id, $text, $explanation, $order, $choices);
            return new JsonResponse(null, Response::HTTP_NO_CONTENT);
        } catch (\InvalidArgumentException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_NOT_FOUND);
        }
    }

    #[Route('/questions/{id}', name: 'api_questions_delete', requirements: ['id' => '[0-9a-fA-F\-]{36}'], methods: ['DELETE'])]
    public function delete(string $id): JsonResponse
    {
        $this->questionService->delete($id);
        return new JsonResponse(null, Response::HTTP_NO_CONTENT);
    }
}
