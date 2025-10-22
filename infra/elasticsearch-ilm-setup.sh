echo "Setting up Elasticsearch ILM policies..."

# 1. Create ILM policy for application logs
curl -X PUT "localhost:9200/_ilm/policy/jerrygram-logs-policy" \
-H "Content-Type: application/json" \
-d '{
  "policy": {
    "phases": {
      "hot": {
        "min_age": "0ms",
        "actions": {
          "rollover": {
            "max_size": "1GB",
            "max_age": "1d",
            "max_docs": 1000000
          },
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
curl -X PUT "localhost:9200/_ilm/policy/jerrygram-events-policy" \
-H "Content-Type: application/json" \
-d '{
  "policy": {
    "phases": {
      "hot": {
        "min_age": "0ms",
        "actions": {
          "rollover": {
            "max_size": "500MB",
            "max_age": "1d",
            "max_docs": 500000
          },
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
curl -X PUT "localhost:9200/_index_template/jerrygram-logs-template" \
-H "Content-Type: application/json" \
-d '{
  "index_patterns": ["jerrygram-dotnet-backend-*"],
  "template": {
    "settings": {
      "number_of_shards": 1,
      "number_of_replicas": 1,
      "index.lifecycle.name": "jerrygram-logs-policy",
      "index.lifecycle.rollover_alias": "jerrygram-logs",
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

curl -X PUT "localhost:9200/_index_template/jerrygram-events-template" \
-H "Content-Type: application/json" \
-d '{
  "index_patterns": ["jerrygram-events-*"],
  "template": {
    "settings": {
      "number_of_shards": 2,
      "number_of_replicas": 0,
      "index.lifecycle.name": "jerrygram-events-policy",
      "index.lifecycle.rollover_alias": "jerrygram-events",
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

# 4. Create initial indices with write aliases
curl -X PUT "localhost:9200/jerrygram-logs-000001" \
-H "Content-Type: application/json" \
-d '{
  "aliases": {
    "jerrygram-logs": {
      "is_write_index": true
    }
  }
}'

curl -X PUT "localhost:9200/jerrygram-events-000001" \
-H "Content-Type: application/json" \
-d '{
  "aliases": {
    "jerrygram-events": {
      "is_write_index": true
    }
  }
}'

echo "Elasticsearch ILM setup completed!"
echo "Logs retention: 90 days with tiering"
echo "Events retention: 30 days with fast search"