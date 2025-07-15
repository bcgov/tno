# Indexer

## Elasticsearch

If the DEV/TEST environments are also migrated the cost per month goes up 2 or 3 times due to similar content in each.

### Document Count

```bash
GET /content/_count
# 3,058,048

GET /unpublished_content/_count
# Elastic: 7,992,737
# Database: 7,979,437

# Total: 11,050,785
```

### Disk Usage

```bash
GET /_cat/nodes?v&h=name,disk.total,disk.used,disk.avail,disk.used_percent

# name      disk.total disk.used disk.avail disk.used_percent
# elastic-0     59.9gb    39.3gb     20.6gb             65.53
# elastic-1     59.9gb    40.6gb     19.3gb             67.75
# elastic-2     59.9gb    40.1gb     19.8gb             66.90
```

### Indices

Nested documents are included in the total count. We have at least 5 nested indexes.

```bash
GET /_cat/indices?v

# health status index                       uuid                   pri rep docs.count docs.deleted store.size pri.store.size
# green  open   .geoip_databases            V-5EDOatT7WEET241b3cww   1   1         40           40     76.1mb           38mb
# green  open   .monitoring-es-7-2025.06.16 SIMRmD4WT6q4Wdz9FWcE4Q   1   1     107043       198604    160.5mb           80mb
# green  open   .monitoring-es-7-2025.06.15 21bz7nYmQ1iuP1ypuPcDHg   1   1     181875            0    196.2mb           98mb
# green  open   .monitoring-es-7-2025.06.14 GxLFaEuLT_OjpgHRdQnlkA   1   1     181958          546    201.4mb        100.6mb
# green  open   .monitoring-es-7-2025.06.13 jgbCEk1GRJOq8OGwNS2Arg   1   1     181880          780    203.1mb        101.5mb
# green  open   .monitoring-es-7-2025.06.12 n0PKP1obRpmNJ7eto3brOw   1   1     181880         1248    205.3mb        102.6mb
# green  open   migrations                  SBSc_k6SRMWr-aQMB7dhnA   1   1          8            0     20.3kb         10.1kb
# green  open   .monitoring-es-7-2025.06.11 XZfuzYlaTke8pILaqhLlig   1   1     181958         1599    204.7mb        102.2mb
# yellow open   unpublished_content_v1.0.10 3BvlwjiITvy1C240U1wkQQ   1   2   32462374      1430959     59.2gb         29.6gb
# green  open   .monitoring-es-7-2025.06.10 fe8ouY-lTgSVcyvqPWnzBA   1   1     181916         1950    202.6mb        101.5mb
# yellow open   content_v1.0.10             Yukgk5SYQ0WVlKhWJ3H0Rg   1   3   13092704       744821     29.2gb          9.7gb
# green  open   .tasks                      xbbjnPgIQhm973OWEQUA2w   1   1         10            0     89.1kb         44.5kb

# content:              13,092,704
# unpublished_content:  32,462,374
```

### Master

```bash
GET /_cat/master

# Ns86LmloTlK2BwCIgY1YZw 10.97.190.214 10.97.190.214 elastic-1
```

### Indexing

```bash
GET /_nodes/stats?pretty

# Start: March 24, 2025, 22:54:01.948 PDT
# Timestamp: May 14, 2025, 08:02:35.504 PST
# index_total: 21,898,362
# query_total: 1,459,695
# read_kilobytes: 29360107 = 29GB
# write_kilobytes: 155330276 = 15GB
# rx_size_in_bytes: 240920437720 = 240GB (inbound)
# tx_size_in_bytes: 327530272552 = 328GB (outbound)
```

### Openshift Resources

Memory: 5G max
CPU: 0.02-0.1 max, 0.5 spike
Network In: 50k max, 300k spike
Network Out: 50k max, 600k peak, 1.6M spike

### Elastic Serverless

How do we calculate VCU?

### Elastic Hosted

Which hardware profile? Storage, CPU, Vector, General

> Azure: Storage, 2 zones, 140GB | 4GB | 2.1 vCPU = $0.9321/hr = $671.112/mth
> Azure: Storage, 2 zones, 280GB | 8GB | 2.1 vCPU = $1.4433/hr = $1,039.18/mth
>
> AWS: Storage, 2 zones, 240GB | 8GB | 2.2 vCPU = $1.2345/hr = $888.84/mth
