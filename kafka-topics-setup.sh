#!/bin/bash

echo "Waiting for Kafka to be ready..."
sleep 60

# Create topics for different event types
echo "Creating Kafka topics..."

docker exec jg-kafka kafka-topics \
  --create \
  --topic search-events \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1 \
  --config cleanup.policy=delete \
  --config retention.ms=604800000

docker exec jg-kafka kafka-topics \
  --create \
  --topic user-events \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1 \
  --config cleanup.policy=delete \
  --config retention.ms=604800000

docker exec jg-kafka kafka-topics \
  --create \
  --topic post-events \
  --bootstrap-server localhost:9092 \
  --partitions 3 \
  --replication-factor 1 \
  --config cleanup.policy=delete \
  --config retention.ms=604800000

docker exec jg-kafka kafka-topics \
  --create \
  --topic popular-searches \
  --bootstrap-server localhost:9092 \
  --partitions 1 \
  --replication-factor 1 \
  --config cleanup.policy=compact

docker exec jg-kafka kafka-topics \
  --create \
  --topic dotnet-application-logs \
  --bootstrap-server localhost:9092 \
  --partitions 2 \
  --replication-factor 1 \
  --config cleanup.policy=delete \
  --config retention.ms=259200000

echo "Kafka topics created successfully!"

# List all topics to verify
echo "Current topics:"
docker exec jg-kafka kafka-topics --list --bootstrap-server localhost:9092