#!/bin/bash
set -e

# Wait until Elasticsearch is available
until curl -s http://elasticsearch:9200; do
  echo "Waiting for Elasticsearch..."
  sleep 2
done

echo "Elasticsearch is up, loading bulk data..."

curl -X PUT "http://elasticsearch:9200/products" -H 'Content-Type: application/json' -d '
{
  "settings": {
    "analysis": {
      "filter": {
        "autocomplete_filter": {
          "type":     "edge_ngram",
          "min_gram": 2,
          "max_gram": 20
        }
      },
      "analyzer": {
        "autocomplete": {
          "type":      "custom",
          "tokenizer": "standard",
          "filter": [
            "lowercase",
            "autocomplete_filter"
          ]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "name": {
        "type":     "text",
        "analyzer": "autocomplete",
        "search_analyzer": "standard"
      },
      "description": {
        "type":     "text"
      },
      "category": {
        "type":     "keyword"
      },
      "brand": {
        "type":     "text",
        "analyzer": "autocomplete",
        "search_analyzer": "standard"
      },
      "tags": {
        "type":     "keyword"
      }
    }
  }
}'

curl -H "Content-Type: application/x-ndjson" -XPOST "http://elasticsearch:9200/products/_bulk" --data-binary @/sample_data/elasticsearch_bulk_5000_products.json

echo "Bulk upload finished"
