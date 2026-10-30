set -euo pipefail
mkdir /tmp/backend
tar -C /workspace/help-center-backend --exclude=.vs --exclude=bin --exclude=obj --exclude=Logs --exclude=Uploads -cf - . | tar -C /tmp/backend -xf -
cp /workspace/.editorconfig /tmp/.editorconfig
if [[ -z "${POSTGRES_PASSWORD:-}" ]]; then
    echo "POSTGRES_PASSWORD is required to connect to the development PostgreSQL database." >&2
    exit 1
fi
export ConnectionStrings__DefaultConnection="Host=db;Port=5432;Database=HelpCenterDb;Username=helpcenter;Password=${POSTGRES_PASSWORD}"
export DOTNET_NOLOGO=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet tool install dotnet-ef --version 9.0.19 --tool-path /tmp/ef
cd /tmp/backend
/tmp/ef/dotnet-ef database update --project HelpCenter.Persistence --startup-project HelpCenter.WebApi --configuration Release
