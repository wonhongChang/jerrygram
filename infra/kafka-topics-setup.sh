#!/bin/bash

echo "Waiting for Kafka to be ready..."
sleep 60

KAFKA_CONTAINER_NAME="${KAFKA_CONTAINER_NAME:-jg-kafka}"
KAFKA_BOOTSTRAP_SERVER="${KAFKA_BOOTSTRAP_SERVER:-kafka:29092}"

# Create topics for different event types
echo "Creating Kafka topics..."

docker exec "${KAFKA_CONTAINER_NAME}" kafka-topics \
  --create \
  --topic search-events \
  --bootstrap-server "${KAFKA_BOOTSTRAP_SERVER}" \
  --partitions 3 \
  --replication-factor 1 \
  --config cleanup.policy=delete \
  --config retention.ms=604800000

docker exec "${KAFKA_CONTAINER_NAME}" kafka-topics \
  --create \
  --topic user-events \
  --bootstrap-server "${KAFKA_BOOTSTRAP_SERVER}" \
  --partitions 3 \
  --replication-factor 1 \
  --config cleanup.policy=delete \
  --config retention.ms=604800000

docker exec "${KAFKA_CONTAINER_NAME}" kafka-topics \
  --create \
  --topic post-events \
  --bootstrap-server "${KAFKA_BOOTSTRAP_SERVER}" \
  --partitions 3 \
  --replication-factor 1 \
  --config cleanup.policy=delete \
  --config retention.ms=604800000

docker exec "${KAFKA_CONTAINER_NAME}" kafka-topics \
  --create \
  --topic popular-searches \
  --bootstrap-server "${KAFKA_BOOTSTRAP_SERVER}" \
  --partitions 1 \
  --replication-factor 1 \
  --config cleanup.policy=compact

echo "Kafka topics created successfully!"

# List all topics to verify
echo "Current topics:"
docker exec "${KAFKA_CONTAINER_NAME}" kafka-topics --list --bootstrap-server "${KAFKA_BOOTSTRAP_SERVER}"
