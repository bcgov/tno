#!/usr/bin/env python3
"""Integration check: build tno:nginx first, then run this with Docker available."""
import json
import subprocess
import time
import urllib.error
import urllib.request
import uuid

prefix = 'tno-nginx-test-' + uuid.uuid4().hex[:8]
containers = []


def docker(*args):
    return subprocess.check_output(['docker', *args], text=True).strip()


def start(name, *args):
    containers.append(name)
    return docker('run', '-d', '--name', name, '--network', prefix, *args)


def backend(alias, suffix=''):
    name = prefix + '-' + alias + suffix
    port = 8080 if alias == 'api' else 3000
    config = f'server {{ listen {port}; location / {{ return 200 "$request_uri"; }} }}'
    start(name, '--network-alias', alias, 'tno:nginx', 'sh', '-c',
          'printf "%s" "$1" > /etc/nginx/conf.d/default.conf; exec nginx -g "daemon off;"',
          'sh', config)
    return name


def expect(port, path, status, body=None):
    deadline = time.monotonic() + 15
    while time.monotonic() < deadline:
        try:
            with urllib.request.urlopen(f'http://127.0.0.1:{port}{path}', timeout=3) as response:
                actual, content = response.status, response.read().decode()
        except urllib.error.HTTPError as error:
            actual, content = error.code, error.read().decode()
        except OSError:
            actual, content = 0, ''
        if actual == status and (body is None or content == body):
            return
        time.sleep(0.25)
    raise AssertionError((path, status, actual, content[:200]))


try:
    docker('network', 'create', prefix)
    proxy = prefix + '-proxy'
    start(proxy, '-p', '127.0.0.1::80', '-p', '127.0.0.1::81', 'tno:nginx')
    ports = [int(docker('port', proxy, str(p)).rsplit(':', 1)[1]) for p in (80, 81)]
    docker('exec', proxy, 'nginx', '-t')
    for port in ports:
        expect(port, '/nginx-status', 200, 'healthy\n')
        expect(port, '/', 502)
    print('PASS: proxy starts with all upstreams missing', flush=True)

    names = {alias: backend(alias) for alias in ('editor', 'subscriber', 'api')}
    for port in ports:
        expect(port, '/page?x=1', 200, '/page?x=1')
        expect(port, '/api/health?x=1', 200, '/health?x=1')
    print('PASS: late startup and API path/query rewriting', flush=True)

    for index, alias in enumerate(('editor', 'subscriber')):
        old = names[alias]
        # Keep the old address occupied but withdraw the upstream DNS alias.
        old_ip = json.loads(docker('inspect', old))[0]['NetworkSettings']['Networks'][prefix]['IPAddress']
        docker('network', 'disconnect', prefix, old)
        docker('network', 'connect', prefix, old)
        expect(ports[index], '/', 502)
        expect(ports[1-index], '/still-up', 200, '/still-up')
        expect(ports[index], '/api/health', 200, '/health')
        replacement = backend(alias, '-replacement')
        new_ip = json.loads(docker('inspect', replacement))[0]['NetworkSettings']['Networks'][prefix]['IPAddress']
        assert old_ip != new_ip, (old_ip, new_ip)
        expect(ports[index], '/recovered', 200, '/recovered')
        print(f'PASS: {alias} outage isolated; recovered at changed IP without proxy restart', flush=True)

    docker('rm', '-f', names['api'])
    for port in ports:
        expect(port, '/api/health', 502)
        expect(port, '/', 200, '/')
    backend('api', '-replacement')
    for port in ports:
        expect(port, '/api/health', 200, '/health')
    assert json.loads(docker('inspect', proxy))[0]['RestartCount'] == 0
    print('PASS: API outage and recovery; proxy never restarted', flush=True)
finally:
    for name in reversed(containers):
        subprocess.run(['docker', 'rm', '-f', name], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    subprocess.run(['docker', 'network', 'rm', prefix], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
