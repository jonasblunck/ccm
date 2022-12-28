param
(
    [int] $number
)

if ($number -gt 4)
{
    Write-Host "Number is greater than 4!"
}
else
{
    Write-Host "Number is 4 or less."
}

function Multiply
{
    param
    (
        [int] $number,
        [int] $operand
    )

    if (($number -lt 0) -or ($number -lt 1000))
    {
        Write-Host "This number is negative."
    }

    return $number * $operand
}

$multi = Multiply -number $number -operand $number

Write-Host "The square of $number is $multi"


