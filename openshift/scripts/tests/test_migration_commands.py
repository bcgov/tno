"""Exercise the real scripts with isolated configuration and mocked external commands.

Run: python3 -m unittest discover -s openshift/scripts/tests -v
No Docker, registry, or OpenShift operations are performed.
"""

import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest


MOCK = r'''#!/usr/bin/env python3
import base64, json, os, pathlib, sys
command = pathlib.Path(sys.argv[0]).name
args = sys.argv[1:]
text = ' '.join(args)
with open(os.environ['CALLS'], 'a') as log:
    log.write(command + ' ' + text + '\n')
if command == 'curl':
    print('200', end='')
elif command == 'oc':
    if args[0] == 'apply':
        pathlib.Path(os.environ['MANIFEST']).write_text(sys.stdin.read())
        sys.exit(1 if os.environ.get('APPLY_FAIL') else 0)
    if args[:2] == ['get', 'deployment']:
        print('example.azurecr.io/nginx:dev')
    elif '.dockerconfigjson' in text:
        print(base64.b64encode(json.dumps({'auths': {'example.azurecr.io': {
            'username': 'mock-user', 'password': 'mock-password'
        }}}).encode()).decode())
    elif args[:2] == ['get', 'statefulset']:
        if 'ConnectionStrings__TNO' in text:
            print('api' if text.endswith('.name}') else 'CONNECTION_STRING')
        elif text.endswith('.name}'):
            print('db-credentials')
        else:
            print('USERNAME' if 'DB_POSTGRES_USERNAME' in text else 'PASSWORD')
    elif '.data.' in text:
        if os.environ.get('MISSING_KEY') in ('', None) or os.environ['MISSING_KEY'] not in text:
            print('mock-value')
    elif '.items[0].metadata.name' in text:
        print('migration-pod')
    elif '.status.phase' in text:
        print('Running')
    elif '.status.succeeded' in text:
        print('0' if os.environ.get('JOB_FAIL') else '1')
    elif '.status.failed' in text:
        print('1' if os.environ.get('JOB_FAIL') else '0')
elif command == 'sleep':
    sys.exit('Unexpected wait in mock deployment')
'''


class MigrationCommands(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        source = Path(__file__).resolve().parents[1]
        self.scripts = self.root / 'openshift' / 'scripts'
        shutil.copytree(source, self.scripts)
        shutil.copy(source.parent / 'Makefile', self.scripts.parent)
        binary = self.root / 'bin'
        binary.mkdir()
        for command in ('docker', 'oc', 'curl', 'skopeo', 'az', 'sleep'):
            path = binary / command
            path.write_text(MOCK)
            path.chmod(0o755)
        self.env = {key: value for key, value in os.environ.items()
                    if not key.startswith(('ACR_', 'ELASTIC_MIGRATION_'))
                    and key not in ('IMAGE', 'DOCKERFILE', 'CONTEXT')}
        self.env.update(PATH=f'{binary}:{os.environ["PATH"]}',
                        CALLS=str(self.root / 'calls'), MANIFEST=str(self.root / 'job.yaml'),
                        ACR_USERNAME='mock-user', ACR_PASSWORD='mock-password',
                        ELASTIC_MIGRATION_WRITERS_PAUSED='true')

    def run_make(self, *args, success=True, **env):
        result = subprocess.run(['make', *args], cwd=self.scripts.parent,
                                env={**self.env, **env}, text=True,
                                stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=15)
        self.assertEqual(result.returncode == 0, success, result.stdout)
        return result.stdout

    def calls(self):
        path = self.root / 'calls'
        return path.read_text() if path.exists() else ''

    def manifest(self):
        return (self.root / 'job.yaml').read_text()

    def test_image_commands(self):
        for target in ('build', 'push', 'pull'):
            self.run_make(target, 'n=elastic-migration', 't=candidate')
        self.run_make('tag', 'n=elastic-migration', 'f=candidate', 't=dev', 'r=local')
        output = self.run_make('tag', 'n=elastic-migration', 'f=candidate', 't=test')
        self.assertIn('make deploy e=test n=elastic-migration t=test', output)
        calls = self.calls()
        self.assertIn('docker build --platform linux/amd64 -t ', calls)
        self.assertIn('-f tools/elastic/migration/Dockerfile .', calls)
        for command in ('push', 'pull', 'tag'):
            self.assertIn(f'docker {command} bcgov-c4awhwfpcremdbga.azurecr.io/elastic-migration:candidate', calls)
        self.assertIn('docker buildx imagetools create --tag ', calls)
        self.assertNotIn('elastic-migration-service', calls)

    def test_up_keeps_local_indexing_off_on_cloud_environments(self):
        for env in ('test', 'prod'):
            self.run_make('up', f'e={env}')
            self.assertIn(f'oc scale deployment indexing-service -n 9b301c-{env} --replicas=0', self.calls())
            count = 2 if env == 'test' else 3
            self.assertIn(f'oc scale deployment indexing-service-cloud -n 9b301c-{env} --replicas={count}', self.calls())

    def test_migration_controls(self):
        self.run_make('deploy', 'n=elastic-migration', 'e=test',
                      MIGRATION_ACTIVE_DEADLINE_SECONDS='43200',
                      MIGRATION_STARTUP_TIMEOUT_SECONDS='1200',
                      ELASTIC_MIGRATION_BASELINE='1.0.10',
                      ELASTIC_MIGRATION_REQUESTS_PER_SECOND='500')
        job = self.manifest()
        self.assertIn('activeDeadlineSeconds: 43200', job)
        for name, value in [('BaselineVersion', '1.0.10'), ('WritersPaused', 'true'),
                            ('ReindexRequestsPerSecond', '500')]:
            self.assertIn(f'name: Elastic__{name}\n              value: "{value}"', job)

    def test_invalid_controls_fail_before_promotion(self):
        for values in ({'ELASTIC_MIGRATION_WRITERS_PAUSED': 'false'},
                       {'ELASTIC_MIGRATION_BASELINE': 'bad'},
                       {'ELASTIC_MIGRATION_REQUESTS_PER_SECOND': '0'},
                       {'MIGRATION_ACTIVE_DEADLINE_SECONDS': '0'},
                       {'MIGRATION_STARTUP_TIMEOUT_SECONDS': '-1'}):
            self.run_make('deploy', 'n=elastic-migration', 'e=dev', success=False, **values)
        self.assertNotIn('imagetools create', self.calls())
        self.assertNotIn('oc apply', self.calls())

    def test_dev_job(self):
        self.run_make('deploy', 'n=elastic-migration', 'e=dev', 't=latest')
        job = self.manifest()
        for value in ('kind: Job', 'backoffLimit: 0', 'activeDeadlineSeconds: 86400', 'image: example.azurecr.io/elastic-migration:dev',
                      'args: []', 'name: indexing-service', 'name: elastic',
                      'name: Elastic__Username', 'name: Elastic__Password', 'name: db-credentials'):
            self.assertIn(value, job)
        self.assertNotIn('Elastic__ApiKey', job)
        self.assertNotIn('mock-value', job)
        self.assertIn('docker://example.azurecr.io/elastic-migration:latest', self.calls())
        self.assertNotIn('rollout', self.calls())

    def test_cloud_job_and_version(self):
        for environment in ('test', 'prod'):
            self.run_make('deploy', 'n=elastic-migration', f'e={environment}', 'm=1.0.11', 's=custom-db')
            job = self.manifest()
            self.assertIn('args: ["--version", "1.0.11"]', job)
            self.assertIn('name: indexing-service-cloud', job)
            self.assertIn('name: elastic-cloud', job)
            self.assertIn('name: custom-db', job)
            self.assertIn('name: Elastic__ApiKey', job)
            self.assertNotIn('Elastic__Username', job)
            self.assertNotIn('Elastic__Password', job)

    def test_database_job_unchanged(self):
        self.run_make('deploy', 'n=db-migration', 'e=dev', 'm=0')
        self.assertIn('args: ["0"]', self.manifest())
        self.assertIn('image: example.azurecr.io/db-migration:dev', self.manifest())
        self.assertNotIn('Elastic__', self.manifest())

    def test_cluster_overrides(self):
        self.run_make('deploy', 'n=elastic-migration', 'e=test',
                      ELASTIC_MIGRATION_CONFIGMAP='alternate-indexing',
                      ELASTIC_MIGRATION_SECRET='alternate-elastic',
                      ELASTIC_MIGRATION_AUTH='basic')
        self.assertIn('name: alternate-indexing', self.manifest())
        self.assertIn('name: alternate-elastic', self.manifest())
        self.assertIn('Elastic__Username', self.manifest())
        self.assertNotIn('Elastic__ApiKey', self.manifest())

    def test_missing_api_key_prevents_promotion(self):
        self.run_make('deploy', 'n=elastic-migration', 'e=test', success=False,
                      MISSING_KEY='ApiKey')
        self.assertNotIn('skopeo copy', self.calls())
        self.assertNotIn('oc apply', self.calls())

    def test_invalid_version_before_external_commands(self):
        self.run_make('deploy', 'n=elastic-migration', 'm=0', success=False)
        self.assertEqual('', self.calls())

    def test_missing_config_prevents_promotion(self):
        self.run_make('deploy', 'n=elastic-migration', success=False, MISSING_KEY='CONTENT_INDEX')
        self.assertNotIn('skopeo copy', self.calls())
        self.assertNotIn('oc apply', self.calls())

    def test_apply_failure_stops(self):
        self.run_make('deploy', 'n=elastic-migration', success=False, APPLY_FAIL='1')
        self.assertNotIn('oc logs', self.calls())

    def test_failed_job_is_reported(self):
        output = self.run_make('deploy', 'n=elastic-migration', success=False, JOB_FAIL='1')
        self.assertIn('did not succeed', output)


if __name__ == '__main__':
    unittest.main()
