$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $projectRoot 'src\OcrTool.Api\OcrTool.Api.csproj'
$output = Join-Path $projectRoot 'artifacts\publish\api-linux-x64'

& (Join-Path $PSScriptRoot 'dotnet.ps1') publish $project `
    --configuration 'Release' `
    --runtime 'linux-x64' `
    --self-contained 'true' `
    --output $output

exit $LASTEXITCODE
