#!/usr/bin/env bash
# Add the same EF migration to sqlite and postgres projects, then update postgres.
# Usage: ./scripts/create-migrations.sh MigrationName
# Requires: dotnet ef (dotnet tool install --global dotnet-ef)

set -euo pipefail

if [[ $# -lt 1 ]] || [[ -z "${1// }" ]]; then
  echo "Usage: $0 <MigrationName>" >&2
  exit 1
fi

MIGRATION_NAME="$1"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

SQLITE_PROJECT="${REPO_ROOT}/src/api/migrations/mark.davison.rome.api.migrations.sqlite/mark.davison.rome.api.migrations.sqlite.csproj"
POSTGRES_PROJECT="${REPO_ROOT}/src/api/migrations/mark.davison.rome.api.migrations.postgres/mark.davison.rome.api.migrations.postgres.csproj"

dotnet ef migrations add "${MIGRATION_NAME}" --project "${SQLITE_PROJECT}"
dotnet ef migrations add "${MIGRATION_NAME}" --project "${POSTGRES_PROJECT}"

if [[ "${APPLY_POSTGRES:-}" == "1" ]] || [[ "${2:-}" == "--update-postgres" ]]; then
  # Requires Postgres: ./scripts/postgres-dev.sh up && export DATABASE__* (see postgres-dev.sh)
  export DATABASE__HOST="${DATABASE__HOST:-localhost}"
  export DATABASE__PORT="${DATABASE__PORT:-5455}"
  export DATABASE__DATABASE="${DATABASE__DATABASE:-PostgresDatabase}"
  export DATABASE__USERNAME="${DATABASE__USERNAME:-romedbuser}"
  export DATABASE__PASSWORD="${DATABASE__PASSWORD:-romedbpassword}"
  dotnet ef database update --project "${POSTGRES_PROJECT}"
else
  echo "Skipping postgres database update. To apply: ./scripts/postgres-dev.sh migrate"
  echo "Or re-run with: APPLY_POSTGRES=1 $0 ${MIGRATION_NAME}"
fi
