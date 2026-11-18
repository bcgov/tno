FROM docker.elastic.co/mcp/elasticsearch:0.4.6

# Install curl for healthcheck (not present in base Wolfi image)
USER root
RUN apk add --no-cache curl

# Transport: "http" (networked, for Azure AI Foundry) or "stdio"
CMD ["http"]
