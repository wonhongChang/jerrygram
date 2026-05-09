#!/bin/bash

set -euo pipefail

ELASTICSEARCH_URL="${ELASTICSEARCH_URL:-http://localhost:${JG_ELASTICSEARCH_PORT:-19200}}"

echo "Setting up Elasticsearch ILM policies at ${ELASTICSEARCH_URL}..."

# 1. Create ILM policy for application logs
curl -X PUT "${ELASTICSEARCH_URL}/_ilm/policy/jerrygram-logs-policy" \
-H "Content-Type: application/json" \
-d '{
  "policy": {
    "phases": {
      "hot": {
        "min_age": "0ms",
        "actions": {
          "set_priority": {
            "priority": 100
          }
        }
      },
      "warm": {
        "min_age": "7d",
        "actions": {
          "set_priority": {
            "priority": 50
          },
          "allocate": {
            "number_of_replicas": 0
          }
        }
      },
      "cold": {
        "min_age": "30d",
        "actions": {
          "set_priority": {
            "priority": 0
          },
          "allocate": {
            "number_of_replicas": 0
          }
        }
      },
      "delete": {
        "min_age": "90d",
        "actions": {
          "delete": {}
        }
      }
    }
  }
}'

# 2. Create ILM policy for event analytics (shorter retention)
curl -X PUT "${ELASTICSEARCH_URL}/_ilm/policy/jerrygram-events-policy" \
-H "Content-Type: application/json" \
-d '{
  "policy": {
    "phases": {
      "hot": {
        "min_age": "0ms",
        "actions": {
          "set_priority": {
            "priority": 100
          }
        }
      },
      "warm": {
        "min_age": "3d",
        "actions": {
          "set_priority": {
            "priority": 50
          },
          "allocate": {
            "number_of_replicas": 0
          },
          "forcemerge": {
            "max_num_segments": 1
          }
        }
      },
      "delete": {
        "min_age": "30d",
        "actions": {
          "delete": {}
        }
      }
    }
  }
}'

# 3. Create index templates with ILM policies
curl -X PUT "${ELASTICSEARCH_URL}/_index_template/jerrygram-logs-template" \
-H "Content-Type: application/json" \
-d '{
  "index_patterns": ["jerrygram-dotnet-backend-*"],
  "template": {
    "settings": {
      "number_of_shards": 1,
      "number_of_replicas": 0,
      "index.lifecycle.name": "jerrygram-logs-policy",
      "refresh_interval": "30s"
    },
    "mappings": {
      "properties": {
        "@timestamp": {
          "type": "date"
        },
        "level": {
          "type": "keyword"
        },
        "message": {
          "type": "text",
          "analyzer": "standard"
        },
        "correlationId": {
          "type": "keyword"
        },
        "userId": {
          "type": "keyword"
        },
        "requestMethod": {
          "type": "keyword"
        },
        "requestPath": {
          "type": "keyword"
        },
        "responseStatusCode": {
          "type": "integer"
        },
        "duration": {
          "type": "long"
        },
        "service": {
          "type": "keyword"
        },
        "environment": {
          "type": "keyword"
        }
      }
    }
  }
}'

curl -X PUT "${ELASTICSEARCH_URL}/_index_template/jerrygram-events-template" \
-H "Content-Type: application/json" \
-d '{
  "index_patterns": ["jerrygram-events-*"],
  "template": {
    "settings": {
      "number_of_shards": 2,
      "number_of_replicas": 0,
      "index.lifecycle.name": "jerrygram-events-policy",
      "refresh_interval": "5s"
    },
    "mappings": {
      "properties": {
        "@timestamp": {
          "type": "date"
        },
        "kafka_topic": {
          "type": "keyword"
        },
        "eventId": {
          "type": "keyword"
        },
        "userId": {
          "type": "keyword"
        },
        "sessionId": {
          "type": "keyword"
        },
        "correlationId": {
          "type": "keyword"
        },
        "searchTerm": {
          "type": "text",
          "fields": {
            "keyword": {
              "type": "keyword"
            }
          }
        },
        "searchType": {
          "type": "keyword"
        },
        "resultCount": {
          "type": "integer"
        },
        "searchDurationMs": {
          "type": "long"
        },
        "postId": {
          "type": "keyword"
        },
        "eventType": {
          "type": "keyword"
        },
        "targetUserId": {
          "type": "keyword"
        },
        "ipAddress": {
          "type": "ip"
        },
        "userAgent": {
          "type": "text",
          "fields": {
            "keyword": {
              "type": "keyword"
            }
          }
        }
      }
    }
  }
}'

echo "Elasticsearch ILM setup completed!"
echo "Logs retention: 90 days with tiering"
echo "Events retention: 30 days with fast search"
