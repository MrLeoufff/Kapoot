<?php

declare(strict_types=1);

namespace App\Enum;

enum QuizStatus: int
{
    case Draft = 0;
    case Published = 1;
}
