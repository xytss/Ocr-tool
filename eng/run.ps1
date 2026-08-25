$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$dotnetScript = Join-Path $PSScriptRoot 'dotnet.ps1'
$project = Join-Path $repositoryRoot 'src\OcrTool.App\OcrTool.App.csproj'

& $dotnetScript run --project $project
