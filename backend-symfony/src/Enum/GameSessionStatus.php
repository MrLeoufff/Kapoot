<?php

declare(strict_types=1);

namespace App\Enum;

enum GameSessionStatus: int
{
    case Waiting = 0;
    case Running = 1;
    case Finished = 2;
}
