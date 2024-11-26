# Create policy for prod to pull images
oc policy add-role-to-user system:image-puller system:serviceaccount:9b301c-prod:default -n 9b301c-tools

# Tag Imagestreams and import them to prod.
oc -n 9b301c-prod tag 9b301c-tools/kafka:dev kafka:prod

# Deploy statefulSets to the prod environment

oc kustomize postgres/crunchy/overlays/prod | oc create -f -
oc kustomize elastic/overlays/prod | oc create -f -

# Create kafka topic

oc rsh -n 9b301c-prod kafka-broker-0 bash -c "/bin/kafka-topics --bootstrap-server kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092 --topic hub --create --partitions 3 --replication-factor 1"

# Once the database, Kafka, and Elasticsearch are running successfully.
# Deploy the application components.

# Nginx pull image & deploy nginx
oc -n 9b301c-tools tag 9b301c-tools/nginx:dev nginx:prod
oc kustomize nginx/overlays/prod | oc create -f -

# API Image tag & deploy API
oc -n 9b301c-tools tag 9b301c-tools/api:dev api:prod
oc kustomize api/overlays/prod | oc create -f -
oc kustomize app/editor/overlays/prod | oc create -f -

# Deploy shared services resources - This will create ingest-storage pvc & services configmap.

oc kustomize shared_resources/overlays/prod | oc create -f -

# Tag all app image for prod
oc -n 9b301c-tools tag 9b301c-tools/syndication-service:dev syndication-service:prod
oc -n 9b301c-tools tag 9b301c-tools/capture-service:dev capture-service:prod
oc -n 9b301c-tools tag 9b301c-tools/clip-service:dev clip-service:prod
oc -n 9b301c-tools tag 9b301c-tools/image-service:dev image-service:prod
oc -n 9b301c-tools tag 9b301c-tools/filemonitor-service:dev filemonitor-service:prod
oc -n 9b301c-tools tag 9b301c-tools/content-service:dev content-service:prod
oc -n 9b301c-tools tag 9b301c-tools/indexing-service:dev indexing-service:prod
oc -n 9b301c-tools tag 9b301c-tools/transcription-service:dev transcription-service:prod
oc -n 9b301c-tools tag 9b301c-tools/nlp-service:dev nlp-service:prod


# Deploy images to prod.
oc kustomize services/syndication/overlays/prod | oc create -f -
oc kustomize services/capture/overlays/prod | oc create -f -
oc kustomize services/clip/overlays/prod | oc create -f -
oc kustomize services/image/overlays/prod | oc create -f -
oc kustomize services/filemonitor/overlays/prod | oc create -f -
oc kustomize services/content/overlays/prod | oc create -f -
oc kustomize services/indexing/overlays/prod | oc create -f -
oc kustomize services/transcription/overlays/prod | oc create -f -
oc kustomize services/nlp/overlays/prod | oc create -f -