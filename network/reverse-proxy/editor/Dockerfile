ARG BASE_IMAGE=nginxinc/nginx-unprivileged:1.24
FROM ${BASE_IMAGE}

COPY ./config /etc/nginx/conf.d
COPY ./nginx.conf /etc/nginx/nginx.conf

# Set permissions for conf.d directory
# RUN chmod -R 777 /etc/nginx/conf.d

EXPOSE 8080
