set -euo pipefail
mkdir /tmp/backend
tar -C /workspace/help-center-backend --exclude=.vs --exclude=bin --exclude=obj --exclude=Logs --exclude=Uploads -cf - . | tar -C /tmp/backend -xf -
cp /workspace/.editorconfig /tmp/.editorconfig
export ConnectionStrings__DefaultConnection="Server=db;Database=HelpCenterDb;User Id=sa;Password=${SA_PASSWORD};TrustServerCertificate=True"
export DOTNET_NOLOGO=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet tool install dotnet-ef --version 9.0.19 --tool-path /tmp/ef
cd /tmp/backend
/tmp/ef/dotnet-ef database update --project HelpCenter.Persistence --startup-project HelpCenter.WebApi --configuration Release
