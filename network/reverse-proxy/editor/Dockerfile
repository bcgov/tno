FROM image-registry.openshift-image-registry.svc:5000/9b301c-tools/nginx-unprivileged:1.20

COPY ./config /etc/nginx/conf.d

# Set permissions for conf.d directory
# RUN chmod -R 777 /etc/nginx/conf.d

EXPOSE 8080
