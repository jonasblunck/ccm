function Write-HostEx
{
    param
    (
        [string] $Output
    )

    Write-Host $Output
}

<#
.SYNOPSIS
Generate a weak password
#>
function New-WeakPwd($NumberOfCharacters = 10)
{
    $Pwd = $null

    for ($i = 1; $i -le $NumberOfCharacters; $i++)
    {
        switch ($(Get-Random -Minimum 1 -Maximum 4))
        {
            #48 -> 57 :: 0 -> 9
            1 {[string]$Pwd += [char]$(Get-Random -Minimum 48 -Maximum 58)}

            #65 -> 90 :: A -> Z
            2 {[string]$Pwd += [char]$(Get-Random -Minimum 65 -Maximum 91)}

            #97 -> 122 :: a -> z
            3 {[string]$Pwd += [char]$(Get-Random -Minimum 97 -Maximum 123)}
        }
    }

    return $Pwd
}

# adding some basic and stupid examples
function Throw-OnNumber
{
    param
    (
        [int] $Max,
        [int] $ThrowOver
    )

    try
    {
        $number = Get-Random -Minimum 1 -Maximum $Max

        if ($number -gt $ThrowOver)
        {
            throw "Making up error $($number)."
        }
    }
    finally
    {
        if (Test-Path "myfile.no")
        {
            Write-Host "Unexpected file."
        }
    }
}
