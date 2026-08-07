set -euo pipefail

runtime_dir="$(mktemp -d "${TMPDIR:-/tmp}/ow3n-test-serve.XXXXXX")"
pg_data="$runtime_dir/postgres"
pg_socket="$runtime_dir/socket"
pg_log="$runtime_dir/postgres.log"
app_log="$runtime_dir/ow3n.log"
stub_log="$runtime_dir/roll-stub.log"
app_pid=""
stub_pid=""
postgres_started=0

cleanup() {
  status=$?
  trap - EXIT INT TERM

  if [[ -n "$app_pid" ]]; then
    kill "$app_pid" 2>/dev/null || true
    wait "$app_pid" 2>/dev/null || true
  fi
  if [[ -n "$stub_pid" ]]; then
    kill "$stub_pid" 2>/dev/null || true
    wait "$stub_pid" 2>/dev/null || true
  fi
  if [[ "$postgres_started" -eq 1 ]]; then
    pg_ctl -D "$pg_data" -m fast -w stop >/dev/null 2>&1 || true
  fi

  rm -rf -- "$runtime_dir"
  echo "OW3N test stack stopped; disposable state removed."
  exit "$status"
}
trap cleanup EXIT
trap 'exit 130' INT TERM

choose_high_port() {
  python3 - <<'PY'
import socket

excluded = {3000, 5098, 5099, 5100, 5111, 7000}
while True:
    with socket.socket() as sock:
        sock.bind(("127.0.0.1", 0))
        port = sock.getsockname()[1]
    if port >= 20000 and port not in excluded:
        print(port)
        break
PY
}

port_is_available() {
  python3 - "$1" <<'PY'
import errno
import socket
import sys

port = int(sys.argv[1])
for family, address in ((socket.AF_INET, "127.0.0.1"), (socket.AF_INET6, "::1")):
    try:
        with socket.socket(family) as sock:
            sock.bind((address, port))
    except OSError as error:
        if family == socket.AF_INET6 and error.errno in (errno.EAFNOSUPPORT, errno.EADDRNOTAVAIL):
            continue
        raise
PY
}

if ! port_is_available 3000; then
  echo "Port 3000 is already in use; refusing to connect OW3N to an unknown roll service." >&2
  exit 1
fi

app_port="$(choose_high_port)"
pg_port="$(choose_high_port)"
mkdir -p "$pg_socket"

initdb --no-locale --encoding=UTF8 --auth=trust -D "$pg_data" >/dev/null
postgres_started=1
pg_ctl -D "$pg_data" -l "$pg_log" \
  -o "-h '' -k '$pg_socket' -p $pg_port" -w start >/dev/null

psql -X -v ON_ERROR_STOP=1 -h "$pg_socket" -p "$pg_port" -d postgres \
  -v pw="disposable-test-only" -f "$OW3N_INIT_DB" >/dev/null
createdb -h "$pg_socket" -p "$pg_port" -O ow3n ow3n

python3 "$OW3N_ROLL_STUB" >"$stub_log" 2>&1 &
stub_pid=$!
for _attempt in $(seq 1 50); do
  if curl --fail --silent --output /dev/null http://127.0.0.1:3000/healthz; then
    break
  fi
  if ! kill -0 "$stub_pid" 2>/dev/null; then
    echo "Roll-service stub exited during startup:" >&2
    tail -n 30 "$stub_log" >&2
    exit 1
  fi
  sleep 0.1
done
if ! curl --fail --silent --output /dev/null http://127.0.0.1:3000/healthz; then
  echo "Roll-service stub did not become ready." >&2
  exit 1
fi

app_url="http://127.0.0.1:$app_port"
mkdir -p "$runtime_dir/home" "$runtime_dir/xdg-cache" "$runtime_dir/xdg-config" "$runtime_dir/xdg-data"
env -i \
  PATH="$PATH" \
  HOME="$runtime_dir/home" \
  XDG_CACHE_HOME="$runtime_dir/xdg-cache" \
  XDG_CONFIG_HOME="$runtime_dir/xdg-config" \
  XDG_DATA_HOME="$runtime_dir/xdg-data" \
  ASPNETCORE_ENVIRONMENT=Development \
  ASPNETCORE_Kestrel__Endpoints__Http__Url="$app_url" \
  DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
  connectionstrings__CharacterDb="Host=$pg_socket;Port=$pg_port;Username=ow3n;Password=disposable-test-only;Database=ow3n" \
  Discord__ClientId=local-browser-test \
  Discord__ClientSecret=local-browser-test-secret \
  DiscordWebhookUrl=http://127.0.0.1:3000/webhook \
  "$OW3N_APP" --testing >"$app_log" 2>&1 &
  # --testing enables OW3N's TEST-ONLY auth bypass: it auto-authenticates requests
  # as auto-generated 3-digit test accounts (0-999, never a real Discord id) and
  # seeds a usable test character for each, so browser tests can reach the roll
  # panel without real Discord OAuth. This flag is the disposable stack's only way
  # in — see RUNBOOK.md and TestingSupport.cs. Never pass it in production.
app_pid=$!

http_status=""
for _attempt in $(seq 1 120); do
  http_status="$(curl --silent --output /dev/null --write-out '%{http_code}' "$app_url/favicon.png" || true)"
  if [[ "$http_status" == 200 ]]; then
    break
  fi
  if ! kill -0 "$app_pid" 2>/dev/null; then
    echo "OW3N exited during startup:" >&2
    tail -n 60 "$app_log" >&2
    exit 1
  fi
  sleep 0.25
done
if [[ "$http_status" != 200 ]]; then
  echo "OW3N did not become HTTP-reachable (last status: ${http_status:-none}):" >&2
  tail -n 60 "$app_log" >&2
  exit 1
fi

root_status="$(curl --silent --output /dev/null --write-out '%{http_code}' "$app_url/" || true)"

migration_count="$(psql -X -At -h "$pg_socket" -p "$pg_port" -U ow3n -d ow3n \
  -c 'SELECT count(*) FROM "__EFMigrationsHistory";')"
if [[ ! "$migration_count" =~ ^[1-9][0-9]*$ ]]; then
  echo "OW3N database migrations were not applied." >&2
  exit 1
fi

echo "OW3N disposable test server is ready."
echo "URL: $app_url/ (root HTTP $root_status; local asset HTTP $http_status)"
echo "PostgreSQL: ephemeral and seeded ($migration_count EF migrations)"
echo "Roll/webhook stub: http://127.0.0.1:3000"
echo "Press Ctrl-C to stop and delete all temporary state."

wait "$app_pid"
