<?php

declare(strict_types=1);

namespace App\Controller\Api;

use App\Application\Service\GameSessionService;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\Routing\Attribute\Route;

#[Route('/api/gamesessions')]
final class GameSessionController extends AbstractController
{
    use \App\Controller\JsonResponseTrait;

    public function __construct(
        private readonly GameSessionService $gameSessionService,
    ) {
    }

    #[Route('', name: 'api_gamesessions_create', methods: ['POST'])]
    public function create(Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $quizId = (string) ($data['quizId'] ?? '');
        $hostId = (string) ($data['hostId'] ?? '');

        try {
            $result = $this->gameSessionService->create($quizId, $hostId);
            return $this->json($result, Response::HTTP_CREATED);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        }
    }

    #[Route('/by-code/{code}', name: 'api_gamesessions_by_code', methods: ['GET'])]
    public function getByCode(string $code): JsonResponse
    {
        $session = $this->gameSessionService->getByCode($code);
        if ($session === null) {
            return $this->json(['error' => 'Partie introuvable.'], Response::HTTP_NOT_FOUND);
        }
        return $this->json($session);
    }

    #[Route('/by-code/{code}/players', name: 'api_gamesessions_players', methods: ['GET'])]
    public function getPlayers(string $code): JsonResponse
    {
        $players = $this->gameSessionService->getPlayersByCode($code);
        return $this->json($players);
    }

    #[Route('/join', name: 'api_gamesessions_join', methods: ['POST'])]
    public function join(Request $request): JsonResponse
    {
        $data = json_decode((string) $request->getContent(), true) ?? [];
        $code = (string) ($data['code'] ?? '');
        $pseudo = (string) ($data['pseudo'] ?? '');
        $userId = isset($data['userId']) ? (string) $data['userId'] : null;

        try {
            $result = $this->gameSessionService->join($code, $pseudo, $userId);
            return $this->json($result);
        } catch (\InvalidArgumentException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        } catch (\RuntimeException $e) {
            return $this->json(['error' => $e->getMessage()], Response::HTTP_BAD_REQUEST);
        }
    }
}
