#!/usr/bin/env bash
set -euo pipefail

repository_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
env_file="$repository_root/.env"

if [[ ! -f "$env_file" ]]; then
    echo "Missing $env_file. Copy .env.example to .env and fill the values." >&2
    exit 1
fi

# Keep the parser deliberately small: values are exported verbatim, so passwords and connection
# strings do not need to be shell-escaped. Comments must occupy their own line.
while IFS= read -r line || [[ -n "$line" ]]; do
    [[ -z "$line" || "$line" == \#* ]] && continue
    [[ "$line" != *=* ]] && continue

    key=${line%%=*}
    value=${line#*=}
    export "$key=$value"
done < "$env_file"

exec "$@"
