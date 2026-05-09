#!/bin/bash

set -euo pipefail

KIBANA_URL="${KIBANA_URL:-http://localhost:${JG_KIBANA_PORT:-15601}}"

echo "Creating Kibana data view at ${KIBANA_URL}..."

if curl -sS "${KIBANA_URL}/api/data_views" \
  -H "kbn-xsrf: jerrygram-setup" |
  grep -Fq '"title":"jerrygram-events-*"'; then
  echo "Jerrygram Events data view already exists."
  exit 0
fi

response="$(
  curl -sS -w "\n%{http_code}" -X POST "${KIBANA_URL}/api/data_views/data_view" \
    -H "Content-Type: application/json" \
    -H "kbn-xsrf: jerrygram-setup" \
    -d '{
      "data_view": {
        "title": "jerrygram-events-*",
        "name": "Jerrygram Events",
        "timeFieldName": "@timestamp"
      }
    }'
)"

http_code="$(printf "%s" "${response}" | tail -n 1)"
body="$(printf "%s" "${response}" | sed '$d')"

if [ "${http_code}" -lt 200 ] || [ "${http_code}" -ge 300 ]; then
  printf "%s\n" "${body}"
  echo "Kibana data view setup failed with HTTP ${http_code}."
  exit 1
fi

echo "Kibana data view setup completed."
