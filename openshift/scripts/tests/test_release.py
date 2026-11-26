"""Execute release scripts with a fake oc: no cluster mutations or credentials."""
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest

import yaml

ROOT = Path(__file__).resolve().parents[3]
TEKTON = ROOT / 'openshift/kustomize/tekton/base'
ROLLOUT = ROOT / 'openshift/scripts/release/rollout.sh'


class ReleaseTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.directory = Path(self.temp.name)
        self.log = self.directory / 'calls'
        self.env = dict(os.environ, PATH=f'{self.directory}:{os.environ["PATH"]}', CALLS=str(self.log))
        self.stub('oc', '''#!/usr/bin/env python3
import json, os, sys
args=sys.argv[1:]
with open(os.environ['CALLS'], 'a') as f: f.write(json.dumps(args)+'\\n')
if os.environ.get('FAIL') and os.environ['FAIL'] in ' '.join(args): sys.exit(1)
if args[0]=='get':
    output=args[args.index('-o')+1] if '-o' in args else ''
    if output=='jsonpath={.status.phase}': print(os.environ.get('PHASE','Succeeded'))
    elif output=='jsonpath={.spec.replicas}': print('3')
    elif output.startswith('go-template='): print('component=api,')
    elif output.startswith('jsonpath={.metadata.generation}'):
        from pathlib import Path
        marker = Path(os.environ['CALLS'] + '.readiness')
        attempts = int(marker.read_text()) if marker.exists() else 0
        marker.write_text(str(attempts + 1))
        if attempts < int(os.environ.get('READINESS_FAILURES', '0')):
            print(os.environ.get('READINESS_ERROR', 'Unable to connect to the server: dial tcp 142.34.194.119:6443: i/o timeout'), file=sys.stderr)
            sys.exit(1)
        if os.environ.get('READINESS_WARNING'):
            print('Warning: test warning', file=sys.stderr)
        print(os.environ.get('READINESS','4|4|3|new|new'))
    elif args[1]=='pods' and output=='name': print('pod/api-0')
''')
        self.stub('sleep', '#!/bin/sh\nexit 0\n')

    def stub(self, name, content):
        path = self.directory / name
        path.write_text(content)
        path.chmod(0o755)

    def run_script(self, script, *args, **env):
        return subprocess.run(['bash', str(script), *args], env=dict(self.env, **env), cwd=ROOT,
                              capture_output=True, text=True, timeout=20)

    def calls(self):
        return [json.loads(line) for line in self.log.read_text().splitlines()]

    def test_statefulset_stops_old_pods_before_image_change_and_restores_capacity(self):
        result = self.run_script(ROLLOUT, 'test', 'statefulset', 'api', '10s', 'registry/api:sha')
        self.assertEqual(result.returncode, 0, result.stderr)
        calls = self.calls()
        stop = next(i for i, c in enumerate(calls) if '--replicas=0' in c)
        wait = next(i for i, c in enumerate(calls) if '--for=delete' in c)
        image = next(i for i, c in enumerate(calls) if c[:2] == ['set', 'image'])
        start = next(i for i, c in enumerate(calls) if '--replicas=3' in c)
        self.assertLess(stop, wait)
        self.assertLess(wait, image)
        self.assertLess(image, start)
        self.assertFalse(any(c[:2] == ['rollout', 'restart'] for c in calls))

    def test_statefulset_failure_is_fatal_and_does_not_start_new_pods(self):
        result = self.run_script(ROLLOUT, 'test', 'statefulset', 'api', '10s', 'registry/api:sha', FAIL='wait pod')
        self.assertNotEqual(result.returncode, 0)
        self.assertFalse(any('--replicas=3' in c for c in self.calls()))

    def test_statefulset_readiness_requires_observed_generation_and_matching_revision(self):
        for readiness in ['4|3|3|new|new', '4|4|3|old|new', '4|4|2|new|new', '4||||']:
            with self.subTest(readiness=readiness):
                result = self.run_script(ROLLOUT, 'test', 'statefulset', 'api', '0s', READINESS=readiness)
                self.assertNotEqual(result.returncode, 0)
                self.assertIn('readiness timed out', result.stderr)

    def test_rollout_does_not_require_jq_in_the_openshift_cli_image(self):
        self.stub('jq', '#!/bin/sh\nexit 99\n')
        for kind, name in [('statefulset', 'api'), ('deployment', 'api-services')]:
            result = self.run_script(ROLLOUT, 'test', kind, name, '10s')
            self.assertEqual(result.returncode, 0, result.stderr)

    def test_statefulset_retries_transient_readiness_errors_without_restarting(self):
        result = self.run_script(ROLLOUT, 'test', 'statefulset', 'api', '10s',
                                 READINESS_FAILURES='2', READINESS_WARNING='1')
        self.assertEqual(result.returncode, 0, result.stderr)
        calls = self.calls()
        queries = [c for c in calls if any('metadata.generation' in a for a in c)]
        self.assertEqual(len(queries), 3)
        self.assertTrue(all(any(a.startswith('--request-timeout=') for a in c) for c in queries))
        self.assertEqual(sum('--replicas=0' in c for c in calls), 1)
        self.assertEqual(sum('--replicas=3' in c for c in calls), 1)
        self.assertIn('retrying', result.stderr)

    def test_statefulset_connection_failures_respect_readiness_deadline(self):
        result = self.run_script(ROLLOUT, 'test', 'statefulset', 'api', '0s', READINESS_FAILURES='100')
        self.assertNotEqual(result.returncode, 0)
        self.assertIn('readiness timed out', result.stderr)
        self.assertFalse(any('wide' in c for c in self.calls()))

    def test_statefulset_permanent_readiness_errors_are_fatal(self):
        result = self.run_script(ROLLOUT, 'test', 'statefulset', 'api', '10s',
                                 READINESS_FAILURES='100', READINESS_ERROR='Error from server (Forbidden): access denied')
        self.assertNotEqual(result.returncode, 0)
        self.assertNotIn('retrying', result.stderr)
        self.assertEqual(sum(any('metadata.generation' in a for a in c) for c in self.calls()), 1)

    def test_deployment_preserves_replicas_and_requires_rollout_success(self):
        result = self.run_script(ROLLOUT, 'test', 'deployment', 'content-service', FAIL='rollout status')
        self.assertNotEqual(result.returncode, 0)
        self.assertFalse(any(c[0] == 'scale' for c in self.calls()))
        patch = next(c for c in self.calls() if c[0] == 'patch')
        strategy = json.loads(patch[patch.index('-p') + 1])['spec']['strategy']
        self.assertEqual(strategy['type'], 'RollingUpdate')
        self.assertEqual(strategy['rollingUpdate'], {'maxSurge': 1, 'maxUnavailable': 0})

    def test_deployment_surge_and_timeout_preserve_availability(self):
        for surge in ['1', '2']:
            with self.subTest(surge=surge):
                self.log.write_text('')
                result = self.run_script(ROLLOUT, 'test', 'deployment', 'content-service',
                                         '600s', MAX_SURGE=surge)
                self.assertEqual(result.returncode, 0, result.stderr)
                calls = self.calls()
                patch = next(c for c in calls if c[0] == 'patch')
                spec = json.loads(patch[patch.index('-p') + 1])['spec']
                self.assertEqual(spec['strategy']['rollingUpdate'],
                                 {'maxSurge': int(surge), 'maxUnavailable': 0})
                self.assertNotIn('replicas', spec)
                status = next(c for c in calls if c[:2] == ['rollout', 'status'])
                self.assertIn('--timeout=600s', status)

    def test_invalid_surge_is_rejected_before_cluster_changes(self):
        for surge in ['0', '-1', '2.5', '25%', '01', '1,"maxUnavailable":6']:
            with self.subTest(surge=surge):
                self.log.write_text('')
                result = self.run_script(ROLLOUT, 'test', 'deployment', 'content-service',
                                         MAX_SURGE=surge)
                self.assertNotEqual(result.returncode, 0)
                self.assertIn('MAX_SURGE must be a positive integer', result.stderr)
                self.assertEqual(self.calls(), [])

    def test_github_migration_timeout_and_failed_pod_block_release(self):
        workflow = yaml.safe_load((ROOT / '.github/workflows/_reusable-db-migration-cd.yml').read_text())
        step = next(s for s in workflow['jobs']['migrate']['steps'] if s.get('name') == 'Run Database Migration')
        script = step['run']
        for key, value in {'env.IMAGE_NAME':'db-migration', 'inputs.openshift_namespace':'test',
                           'env.ACR_URL':'registry', 'inputs.image_tag':'test', 'inputs.db_secret_name':'db'}.items():
            script = script.replace('${{ ' + key + ' }}', value)
        path = self.directory / 'migration.sh'
        path.write_text('set -e\n' + script)
        for phase in ['Pending', 'Failed', 'Succeeded']:
            with self.subTest(phase=phase):
                result = self.run_script(path, PHASE=phase)
                self.assertEqual(result.returncode == 0, phase == 'Succeeded', result.stdout + result.stderr)

    def test_github_services_and_frontend_depend_on_both_apis(self):
        jobs = yaml.safe_load((ROOT / '.github/workflows/deploy-all.yml').read_text())['jobs']
        for name in ['deploy-services', 'deploy-frontend']:
            self.assertIn('deploy-api', jobs[name]['needs'])
            self.assertIn('db-migration', jobs[name]['needs'])
            self.assertNotIn('deploy-services', jobs[name]['needs'])
            self.assertIn("needs.deploy-api.result == 'success'", jobs[name]['if'])
            self.assertIn("needs.detect-changes.outputs.has_builds == 'false'", jobs[name]['if'])
        api = next(s for s in jobs['deploy-api']['steps'] if s.get('name') == 'Deploy API workloads')
        self.assertEqual(api['with']['resource_name'], 'api-services')
        self.assertEqual(api['with']['statefulset_name'], 'api')

    def test_tekton_migration_is_before_deployment_and_webhooks_disabled(self):
        for filename in ['buildah-all', 'deploy-all']:
            pipeline = yaml.safe_load((TEKTON / f'pipelines/{filename}.yaml').read_text())
            tasks = {t['name']: t for t in pipeline['spec']['tasks']}
            self.assertEqual(tasks['deploy-all']['runAfter'], ['db-migration'])
        for env in ['dev', 'test']:
            listener = yaml.safe_load((TEKTON / f'triggers/{env}/listener.yaml').read_text())
            cel = next(i for i in listener['spec']['triggers'][0]['interceptors'] if i['ref']['name'] == 'cel')
            self.assertEqual(next(p['value'] for p in cel['params'] if p['name'] == 'filter'), 'false')

    def test_tekton_both_api_failures_prevent_all_other_rollouts(self):
        task = yaml.safe_load((TEKTON / 'tasks/deploy-all.yaml').read_text())
        script = next(s['script'] for s in task['spec']['steps'] if s['name'] == 'deploy')
        (self.directory / 'build.env').write_text('BUILD_API=true\nBUILD_CONTENT=true\nBUILD_APP=true\n')
        rollout = self.directory / 'repo/openshift/scripts/release/rollout.sh'
        rollout.parent.mkdir(parents=True)
        rollout.write_text("""#!/usr/bin/env bash
set -e
echo "$2/$3" >> "$CALLS"
[[ "$2/$3" != "$FAIL" ]]
""")
        for key, value in {'workspaces.conditions.path':str(self.directory), 'workspaces.source.path':str(self.directory),
                           'params.CONTEXT':'repo', 'params.DEPLOY':'*', 'params.PROJECT_SHORTNAME':'project',
                           'params.ENV':'test', 'params.TIMEOUT':'10s', 'params.ACR_REGISTRY':'registry'}.items():
            script = script.replace('$(' + key + ')', value)
        path = self.directory / 'deploy.sh'
        path.write_text(script)
        for failure in ['statefulset/api', 'deployment/api-services', 'deployment/content-service', 'none']:
            with self.subTest(failure=failure):
                self.log.write_text('')
                result = self.run_script(path, FAIL=failure)
                calls = self.log.read_text().splitlines()
                self.assertEqual(result.returncode == 0, failure == 'none', result.stderr)
                self.assertEqual(calls[0], 'statefulset/api')
                if failure == 'statefulset/api':
                    self.assertEqual(len(calls), 1)
                elif failure == 'deployment/api-services':
                    self.assertEqual(calls, ['statefulset/api', 'deployment/api-services'])
                else:
                    self.assertEqual(calls[1], 'deployment/api-services')
                    self.assertEqual(set(calls[2:]), {'deployment/content-service', 'deployment/editor', 'deployment/subscriber'})

    def test_tekton_build_paths_exist_and_credentials_are_not_traced(self):
        task = yaml.safe_load((TEKTON / 'tasks/build-all.yaml').read_text())
        script = task['spec']['steps'][0]['script']
        conditions = self.directory / 'build.env'
        conditions.write_text('')
        for key, value in {'workspaces.conditions.path':str(self.directory), 'params.CONTEXT':str(ROOT),
                           'params.BUILD':'all', 'params.IMAGE_REGISTRY':'registry',
                           'params.STORAGE_DRIVER':'vfs', 'params.IMAGE_TAG':'sha'}.items():
            script = script.replace('$(' + key + ')', value)
        path = self.directory / 'build.sh'
        path.write_text(script)
        self.stub('buildah', '''#!/usr/bin/env python3
import pathlib, sys
args=sys.argv[1:]
if 'bud' in args:
    assert pathlib.Path(args[args.index('-f')+1]).is_file(), args
    assert pathlib.Path(args[-1]).is_dir(), args
''')
        result = self.run_script(path, IMAGE_REGISTRY_USER='user', IMAGE_REGISTRY_PASS='secret-canary')
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertNotIn('secret-canary', result.stdout + result.stderr)


if __name__ == '__main__':
    unittest.main()
