# Elasticsearch

Generate a password file.

```bash
echo PASSWORD=$(< /dev/urandom tr -dc 'A-Z-a-z-0-9@#$%&_+=' | head -c${1:-32}) > ./base/secret.env
```

Install Elasticsearch cluster in specified environment.

```bash
oc kustomize ./overlays/dev | oc create -f -
```

View Elasticsearch status [https://tno-dev.apps.silver.devops.gov.bc.ca/elastic](https://tno-dev.apps.silver.devops.gov.bc.ca/elastic)

## Helpful Examples

- [Blog](https://portworx.com/blog/run-ha-elasticsearch-elk-red-hat-openshift/)
- [Blog](https://blog.knoldus.com/deploying-elasticsearch-on-kubernetes/)
- [GitHub](https://github.com/pires/kubernetes-elasticsearch-cluster/blob/master/es-master.yaml)
- [Helm](https://spot.io/blog/kubernetes-tutorial-successful-deployment-of-elasticsearch/)
