$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repositoryRoot
try {
    if (-not (Test-Path -LiteralPath '.env')) {
        throw 'Missing root .env. Copy .env.example to .env and fill in the required values.'
    }
    docker compose --env-file .env -f dockerfiles/docker-compose.dev.yml up -d --build db
    if ($LASTEXITCODE -ne 0) { throw 'Failed to start the development database.' }
    docker run --rm --network helpcenterdev_helpcenter-network --env-file .env -e ASPNETCORE_ENVIRONMENT=Development --mount "type=bind,source=$repositoryRoot,target=/workspace,readonly" mcr.microsoft.com/dotnet/sdk:8.0 bash /workspace/scripts/migrate-dev.sh
    if ($LASTEXITCODE -ne 0) { throw 'Failed to apply database migrations.' }
    docker compose --env-file .env -f dockerfiles/docker-compose.dev.yml up -d --build helpcenter.webapi
    if ($LASTEXITCODE -ne 0) { throw 'Failed to start the API.' }
    if (-not (Test-Path -LiteralPath 'help-center-ui/node_modules/next')) {
        npm --prefix help-center-ui ci --legacy-peer-deps
        if ($LASTEXITCODE -ne 0) { throw 'Failed to install frontend dependencies.' }
    }
    $ready = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        try {
            $response = Invoke-WebRequest 'http://localhost:5005/health/ready' -UseBasicParsing -TimeoutSec 2
            if ($response.StatusCode -eq 200) { $ready = $true; break }
        } catch { }
        Start-Sleep -Seconds 1
    }
    if (-not $ready) { throw 'API database readiness check failed.' }
    npm --prefix help-center-ui run dev
    if ($LASTEXITCODE -ne 0) { throw 'Failed to start the frontend.' }
} finally {
    Pop-Location
}
