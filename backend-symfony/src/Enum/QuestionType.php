<?php

declare(strict_types=1);

namespace App\Enum;

enum QuestionType: int
{
    case Mcq = 0;
    case TrueFalse = 1;
    case DragDrop = 2;
}
