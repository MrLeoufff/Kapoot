<?php

declare(strict_types=1);

namespace App\Controller\Api;

use App\Application\Service\GameRealtimeService;
use App\Entity\User;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Mercure\HubInterface;
use Symfony\Component\Routing\Attribute\Route;
use Symfony\Component\Security\Core\Authentication\Token\Storage\TokenStorageInterface;
use Symfony\Component\Security\Http\Attribute\IsGranted;

#[Route('/api/game')]
final class GameRealtimeController extends AbstractController
{
    use \App\Controller\JsonResponseTrait;

    public function __construct(
        private readonly GameRealtimeService $gameRealtimeService,
        private readonly HubInterface $mercureHub,
        private readonly TokenStorageInterface $tokenStorage,
    ) {
    }

    private function getHostId(): string
    {
        $token = $this->tokenStorage->getToken();
        $user = $token?->getUser();
        return $user instanceof User ? $user->getId() : (string) ($user?->getUserIdentifier() ?? '');
    }

    #[Route('/mercure-url', name: 'api_game_mercure_url', methods: ['GET'])]
    public function mercureUrl(): JsonResponse
    {
        return $this->json([
            'backend' => 'symfony',
            'mercureUrl' => $this->mercureHub->getPublicUrl(),
        ]);
    }

    #[Route('/start', name: 'api_game_start', methods: ['POST'])]
    #[IsGranted('ROLE_USER')]
    public function start(Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $sessionId = (string) ($data['sessionId'] ?? '');

        try {
            $hostId = $this->getHostId();
            $this->gameRealtimeService->startGame($sessionId, $hostId);
            return $this->json(['ok' => true]);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        }
    }

    #[Route('/show-question', name: 'api_game_show_question', methods: ['POST'])]
    #[IsGranted('ROLE_USER')]
    public function showQuestion(Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $sessionId = (string) ($data['sessionId'] ?? '');
        $questionIndex = (int) ($data['questionIndex'] ?? -1);

        try {
            $this->gameRealtimeService->showQuestion($sessionId, $this->getHostId(), $questionIndex);
            return $this->json(['ok' => true]);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        }
    }

    #[Route('/submit-answer', name: 'api_game_submit_answer', methods: ['POST'])]
    public function submitAnswer(Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $sessionId = (string) ($data['sessionId'] ?? '');
        $playerId = (string) ($data['playerId'] ?? '');
        $questionId = (string) ($data['questionId'] ?? '');
        $choiceIds = $data['choiceIds'] ?? [];
        if (!is_array($choiceIds)) {
            $choiceIds = [];
        }
        $choiceIds = array_map('strval', array_values($choiceIds));

        try {
            $this->gameRealtimeService->submitAnswer($sessionId, $playerId, $questionId, $choiceIds);
            return $this->json(['ok' => true]);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        }
    }

    #[Route('/end-question', name: 'api_game_end_question', methods: ['POST'])]
    #[IsGranted('ROLE_USER')]
    public function endQuestion(Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $sessionId = (string) ($data['sessionId'] ?? '');
        $questionId = (string) ($data['questionId'] ?? '');
        $explanation = isset($data['explanation']) ? (string) $data['explanation'] : null;

        try {
            $this->gameRealtimeService->endQuestion($sessionId, $this->getHostId(), $questionId, $explanation);
            return $this->json(['ok' => true]);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        }
    }

    #[Route('/end', name: 'api_game_end', methods: ['POST'])]
    #[IsGranted('ROLE_USER')]
    public function end(Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $sessionId = (string) ($data['sessionId'] ?? '');

        try {
            $this->gameRealtimeService->endGame($sessionId, $this->getHostId());
            return $this->json(['ok' => true]);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        }
    }

    #[Route('/player-left', name: 'api_game_player_left', methods: ['POST'])]
    public function playerLeft(Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $playerId = (string) ($data['playerId'] ?? '');

        try {
            $this->gameRealtimeService->setPlayerHasLeft($playerId, true);
            return $this->json(['ok' => true]);
        } catch (\Throwable $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        }
    }
}
