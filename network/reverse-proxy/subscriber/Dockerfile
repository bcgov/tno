FROM image-registry.openshift-image-registry.svc:5000/9b301c-tools/nginx-unprivileged:1.20

COPY ./config /etc/nginx/conf.d
COPY ./nginx.conf /etc/nginx/nginx.conf

EXPOSE 8080
