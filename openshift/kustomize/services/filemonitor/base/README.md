# Filemonitor service create ssh key secret. Shared with the image service.

The filemonitor service needs a ssh private key secret.

```
kubectl create secret generic ssh-key \
  --from-file=id_rsa=./{id_rsa_file}
```
