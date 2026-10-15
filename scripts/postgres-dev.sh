#!/usr/bin/env bash
# Postgres container + EF migrations against mark.davison.rome.api.migrations.postgres
#
# Design-time config: section "DATABASE" (HOST, PORT, DATABASE, USERNAME, PASSWORD)
# via env DATABASE__* — see PostgresContextFactory / zeno-dotnet-common.
#
# Usage:
#   ./scripts/postgres-dev.sh up          # start container
#   ./scripts/postgres-dev.sh migrate     # apply all pending migrations
#   ./scripts/postgres-dev.sh verify      # connectivity + migration list
#   ./scripts/postgres-dev.sh test-cycle  # add smoke migration, update, remove (no commit)
#   ./scripts/postgres-dev.sh down        # stop container
#   ./scripts/postgres-dev.sh reset       # force stop/remove (same as down; drops ephemeral DB)

set -euo pipefail

export PATH="${PATH}:${HOME}/.dotnet/tools"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
COMPOSE_FILE="${REPO_ROOT}/docker/docker-compose.postgres.yml"

CONTAINER_NAME="rome-postgres-dev"
LEGACY_VOLUME_NAME="rome-postgres-data"

POSTGRES_PROJECT="${REPO_ROOT}/src/api/migrations/mark.davison.rome.api.migrations.postgres/mark.davison.rome.api.migrations.postgres.csproj"
SQLITE_PROJECT="${REPO_ROOT}/src/api/migrations/mark.davison.rome.api.migrations.sqlite/mark.davison.rome.api.migrations.sqlite.csproj"

export DATABASE__HOST="${DATABASE__HOST:-localhost}"
export DATABASE__PORT="${DATABASE__PORT:-5455}"
export DATABASE__DATABASE="${DATABASE__DATABASE:-PostgresDatabase}"
export DATABASE__USERNAME="${DATABASE__USERNAME:-romedbuser}"
export DATABASE__PASSWORD="${DATABASE__PASSWORD:-romedbpassword}"

USE_COMPOSE=0
if docker compose version &>/dev/null 2>&1; then
  COMPOSE=(docker compose -f "${COMPOSE_FILE}")
  USE_COMPOSE=1
elif command -v podman-compose &>/dev/null; then
  COMPOSE=(podman-compose -f "${COMPOSE_FILE}")
  USE_COMPOSE=1
elif podman compose version &>/dev/null 2>&1; then
  COMPOSE=(podman compose -f "${COMPOSE_FILE}")
  USE_COMPOSE=1
fi

container_running() {
  podman ps --format '{{.Names}}' 2>/dev/null | grep -qx "${CONTAINER_NAME}" || \
    docker ps --format '{{.Names}}' 2>/dev/null | grep -qx "${CONTAINER_NAME}"
}

wait_for_postgres() {
  echo "Waiting for Postgres on ${DATABASE__HOST}:${DATABASE__PORT}..."
  for _ in $(seq 1 60); do
    if command -v pg_isready &>/dev/null && pg_isready -h "${DATABASE__HOST}" -p "${DATABASE__PORT}" -U "${DATABASE__USERNAME}" &>/dev/null; then
      echo "Postgres is ready."
      return 0
    fi
    if container_running && podman exec "${CONTAINER_NAME}" pg_isready -U "${DATABASE__USERNAME}" -d "${DATABASE__DATABASE}" &>/dev/null; then
      echo "Postgres is ready."
      return 0
    fi
    sleep 1
  done
  echo "Postgres did not become ready in time." >&2
  exit 1
}

cmd_up() {
  if container_running; then
    echo "Container ${CONTAINER_NAME} already running."
    wait_for_postgres
    return 0
  fi

  if [[ "${USE_COMPOSE}" == "1" ]]; then
    "${COMPOSE[@]}" up -d
  else
    echo "Using podman run (ephemeral DB; install compose for docker-compose.postgres.yml)."
    podman run -d \
      --name "${CONTAINER_NAME}" \
      -p "${DATABASE__PORT}:5432" \
      -e POSTGRES_USER="${DATABASE__USERNAME}" \
      -e POSTGRES_PASSWORD="${DATABASE__PASSWORD}" \
      -e POSTGRES_DB="${DATABASE__DATABASE}" \
      docker.io/library/postgres:16-alpine
  fi
  wait_for_postgres
}

cmd_down() {
  if [[ "${USE_COMPOSE}" == "1" ]]; then
    "${COMPOSE[@]}" down
  elif container_running; then
    podman stop "${CONTAINER_NAME}" && podman rm "${CONTAINER_NAME}"
  else
    echo "No container to stop."
  fi
}

cmd_reset() {
  cmd_down 2>/dev/null || true
  podman rm -f "${CONTAINER_NAME}" 2>/dev/null || true
  # Remove old named volume if it exists from an earlier script version
  podman volume rm "${LEGACY_VOLUME_NAME}" 2>/dev/null || true
}

cmd_migrate() {
  if ! container_running; then
    echo "Postgres is not running. Run: $0 up" >&2
    exit 1
  fi
  dotnet ef database update --project "${POSTGRES_PROJECT}"
}

cmd_verify() {
  if ! container_running; then
    echo "Container not running. Run: $0 up" >&2
    exit 1
  fi
  wait_for_postgres
  podman exec "${CONTAINER_NAME}" psql -U "${DATABASE__USERNAME}" -d "${DATABASE__DATABASE}" -c "SELECT 1 AS ok;"
  echo "--- EF migrations (postgres project) ---"
  dotnet ef migrations list --project "${POSTGRES_PROJECT}"
}

cmd_test_cycle() {
  local smoke_name="WorkflowSmokeTest"
  echo "=== 1/4 Apply existing migrations ==="
  cmd_migrate
  echo "=== 2/4 Add smoke migration (sqlite + postgres) ==="
  dotnet ef migrations add "${smoke_name}" --project "${SQLITE_PROJECT}"
  dotnet ef migrations add "${smoke_name}" --project "${POSTGRES_PROJECT}"
  echo "=== 3/4 Apply smoke migration to Postgres ==="
  cmd_migrate
  echo "=== 4/4 Remove smoke migration (both providers) ==="
  dotnet ef migrations remove --project "${POSTGRES_PROJECT}" --force
  dotnet ef migrations remove --project "${SQLITE_PROJECT}" --force
  cmd_migrate
  echo "Smoke cycle complete. Migration files were not committed."
}

usage() {
  grep '^#   ' "$0" | sed 's/^#   //'
  exit 1
}

case "${1:-}" in
  up) cmd_up ;;
  down) cmd_down ;;
  reset) cmd_reset ;;
  migrate) cmd_migrate ;;
  verify) cmd_verify ;;
  test-cycle) cmd_test_cycle ;;
  *) usage ;;
esac
