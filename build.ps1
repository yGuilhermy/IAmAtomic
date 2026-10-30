param(
    [string]$SdkDir = $env:MOD_SDK_DIR,
    [switch]$IncludeLocalAudio
)

$ErrorActionPreference = 'Stop'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'Instale o SDK .NET 8 ou superior: https://dotnet.microsoft.com/download'
}
$compatibleSdk = @(& dotnet --list-sdks | Where-Object { $_ -match '^(\d+)\.' -and [int]$Matches[1] -ge 8 })
if ($compatibleSdk.Count -eq 0) {
    throw 'Instale o SDK .NET 8 ou superior: https://dotnet.microsoft.com/download'
}
if (-not $SdkDir) {
    $SdkDir = Join-Path $PSScriptRoot 'sdk'
}
if (-not (Test-Path -LiteralPath $SdkDir -PathType Container)) {
    throw 'Informe -SdkDir com as bibliotecas de referencia do jogo ou SDK compativel.'
}
$SdkDir = (Resolve-Path -LiteralPath $SdkDir).Path
$project = Join-Path $PSScriptRoot 'IAmAtomic.csproj'
& dotnet build $project -c Release "-p:SdkDir=$SdkDir"
if ($LASTEXITCODE -ne 0) { throw 'A compilacao falhou.' }
& dotnet run --project (Join-Path $PSScriptRoot 'tests/LogicTests.csproj') -c Release
if ($LASTEXITCODE -ne 0) { throw 'Os testes falharam.' }
$package = Join-Path $PSScriptRoot 'dist/IAmAtomic'
New-Item -ItemType Directory -Path $package -Force | Out-Null
try {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'bin/Release/netstandard2.1/IAmAtomic.dll') -Destination $package -Force
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'manifest.json') -Destination $package -Force
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'LICENSE') -Destination $package -Force
} catch {
    throw "Build e testes concluidos, mas nao foi possivel empacotar. Feche processos que estejam usando dist e tente novamente. $($_.Exception.Message)"
}
if ($IncludeLocalAudio) {
    $audioSource = Join-Path $PSScriptRoot 'audio'
    $requiredAudio = @('atomic_charge.ogg', 'atomic_blast.ogg', 'atomic_impact.ogg')
    foreach ($file in $requiredAudio) {
        if (-not (Test-Path -LiteralPath (Join-Path $audioSource $file))) {
            throw "Audio local ausente: $file. Use apenas arquivos com permissao de uso."
        }
    }
    $audioTarget = Join-Path $package 'audio'
    New-Item -ItemType Directory -Path $audioTarget -Force | Out-Null
    try {
        foreach ($file in $requiredAudio) {
            Copy-Item -LiteralPath (Join-Path $audioSource $file) -Destination $audioTarget -Force
        }
    } catch {
        throw "Nao foi possivel copiar os audios locais. Feche processos usando dist e tente novamente. $($_.Exception.Message)"
    }
} else {
    if (Test-Path -LiteralPath (Join-Path $package 'audio')) {
        throw 'dist contem audio de um build anterior. Use uma pasta dist nova antes de gerar um pacote publico sem audio.'
    }
    Write-Host 'Pacote sem audio. Os OGG locais estao excluidos da publicacao.'
}
Write-Host "Build e testes concluidos. Pacote: $package"
