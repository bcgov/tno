"""Exercise the Kafka topic migration scripts with mocked 'oc' and 'docker'.

Run: python3 -m unittest discover -s openshift/scripts/tests -v
No broker, cluster, or container is touched.
"""

import os
from pathlib import Path
import subprocess
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[3]
MIGRATION = ROOT / 'db/kafka/scripts/migration.sh'
TOPIC_ADD = ROOT / 'db/kafka/scripts/topic-add.sh'
TOPIC_DELETE = ROOT / 'db/kafka/scripts/topic-delete.sh'

# Topics that exist are listed in EXISTING (comma-separated); PARTITIONS_<topic> sets the partition
# count 'describe' reports (6 by default).
MOCK = r'''#!/usr/bin/env python3
import os, pathlib, sys, time
command = pathlib.Path(sys.argv[0]).name
args = sys.argv[1:]
with open(os.environ['CALLS'], 'a') as log:
    log.write(command + ' ' + ' '.join(args) + '\n')
if command == 'oc' and args[:1] == ['whoami']:
    sys.exit(1 if os.environ.get('LOGGED_OUT') else 0)
if '--list' in args:
    print('\n'.join(t for t in os.environ.get('EXISTING', '').split(',') if t))
elif '--describe' in args:
    topic = args[args.index('--topic') + 1]
    partitions = os.environ.get('PARTITIONS_' + topic, '6')
    print(f'Topic: {topic}\tTopicId: abc\tPartitionCount: {partitions}\tReplicationFactor: 3\tConfigs: ')
    # A real broker prints a line per partition after the topic line; a reader that stops early
    # (head, grep -q) must not fail the script with SIGPIPE.
    sys.stdout.flush()
    time.sleep(0.2)
    try:
        for partition in range(int(partitions)):
            print(f'\tTopic: {topic}\tPartition: {partition}\tLeader: 1\tReplicas: 1,2,3\tIsr: 1,2,3', flush=True)
    except BrokenPipeError:
        sys.exit(141)
'''


class KafkaMigration(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        bin_dir = Path(self.temp.name) / 'bin'
        bin_dir.mkdir()
        for name in ('oc', 'docker'):
            path = bin_dir / name
            path.write_text(MOCK)
            path.chmod(0o755)
        self.calls = Path(self.temp.name) / 'calls.log'
        self.env = {
            key: value for key, value in os.environ.items()
            if not key.startswith('KAFKA_')
        }
        self.env.update({'PATH': f'{bin_dir}:{os.environ["PATH"]}', 'CALLS': str(self.calls)})

    def run_script(self, script, *args, **env):
        result = subprocess.run(
            ['bash', str(script), *args],
            env={**self.env, **env},
            capture_output=True,
            text=True,
            cwd=self.temp.name,
            stdin=subprocess.DEVNULL,
        )
        calls = self.calls.read_text().splitlines() if self.calls.exists() else []
        return result, calls

    def changes(self, calls):
        return [c for c in calls if '--create' in c or '--alter' in c or '--delete' in c]

    def test_applies_every_migration_to_the_openshift_namespace(self):
        result, calls = self.run_script(MIGRATION, '-e', 'dev', '-y', EXISTING='index,analysis')
        self.assertEqual(result.returncode, 0, result.stderr)
        creates = [c for c in calls if '--create' in c]
        self.assertTrue(all(c.startswith('oc exec -n 9b301c-dev kafka-broker-0 -- kafka-topics') for c in creates))
        self.assertTrue(all('--bootstrap-server kafka-headless:29092' in c for c in creates))
        self.assertTrue(all('--partitions 6 --replication-factor 3' in c for c in creates))
        created = {c.split('--topic ')[1].split()[0] for c in creates}
        self.assertIn('analysis-backfill', created)
        self.assertIn('work-order', created)
        self.assertNotIn('index', created, 'existing topics are not created again')
        self.assertFalse([c for c in calls if '--alter' in c], 'the environment default never resizes an existing topic')
        self.assertLess(result.stdout.index('== V1.0.0'), result.stdout.index('== V1.0.1'))

    def test_applies_one_version(self):
        result, calls = self.run_script(MIGRATION, '-e', 'test', '-n', '1.0.1', '-y')
        self.assertEqual(result.returncode, 0, result.stderr)
        created = {c.split('--topic ')[1].split()[0] for c in calls if '--create' in c}
        self.assertEqual(created, {'analysis-backfill', 'analysis-retry', 'analysis-dlq', 'work-order'})
        self.assertIn('-n 9b301c-test', calls[-1])

    def test_per_topic_overrides(self):
        result, calls = self.run_script(
            MIGRATION, '-e', 'prod', '-n', '1.0.1', '-y',
            EXISTING='analysis-backfill,analysis-retry',
            KAFKA_TOPIC_ANALYSIS_BACKFILL_PARTITIONS='12',
            KAFKA_TOPIC_ANALYSIS_RETRY_PARTITIONS='3',
            KAFKA_TOPIC_ANALYSIS_DLQ_CONFIG='retention.ms=2592000000',
            KAFKA_TOPIC_WORK_ORDER_REPLICATION_FACTOR='2',
        )
        self.assertEqual(result.returncode, 0, result.stderr)
        changes = self.changes(calls)
        self.assertTrue(any('--alter --topic analysis-backfill --partitions 12' in c for c in changes))
        self.assertFalse(any('analysis-retry' in c for c in changes), 'partitions are never decreased')
        self.assertIn('cannot be decreased', result.stdout)
        self.assertTrue(any('--topic analysis-dlq' in c and '--config retention.ms=2592000000' in c for c in changes))
        self.assertTrue(any('--topic work-order' in c and '--replication-factor 2' in c for c in changes))

    def test_command_line_partitions_apply_to_new_topics(self):
        result, calls = self.run_script(MIGRATION, '-e', 'dev', '-n', '1.0.1', '-p', '9', '--replication', '2', '-y')
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertTrue(all('--partitions 9 --replication-factor 2' in c for c in self.changes(calls)))

    def test_dry_run_changes_nothing(self):
        result, calls = self.run_script(MIGRATION, '-e', 'prod', '-d', EXISTING='hub')
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(self.changes(calls), [])
        self.assertIn('[dry run] kafka-topics --create', result.stdout)

    def test_rollback_needs_a_version(self):
        result, calls = self.run_script(MIGRATION, '-e', 'dev', '-r', '-y')
        self.assertNotEqual(result.returncode, 0)
        self.assertIn('-n', result.stderr)
        self.assertEqual(self.changes(calls), [])

    def test_rollback_deletes_the_versions_topics(self):
        result, calls = self.run_script(MIGRATION, '-e', 'dev', '-r', '-n', '1.0.1', '-y', EXISTING='analysis-backfill,work-order,index')
        self.assertEqual(result.returncode, 0, result.stderr)
        deleted = {c.split('--topic ')[1].split()[0] for c in calls if '--delete' in c}
        self.assertEqual(deleted, {'analysis-backfill', 'work-order'})

    def test_openshift_asks_for_confirmation(self):
        result, calls = self.run_script(MIGRATION, '-e', 'prod')
        self.assertNotEqual(result.returncode, 0)
        self.assertEqual(self.changes(calls), [])

    def test_requires_an_openshift_login(self):
        result, calls = self.run_script(MIGRATION, '-e', 'dev', '-y', LOGGED_OUT='1')
        self.assertNotEqual(result.returncode, 0)
        self.assertIn('oc login', result.stderr)
        self.assertEqual(self.changes(calls), [])

    def test_unknown_environment(self):
        result, _ = self.run_script(MIGRATION, '-e', 'staging', '-y')
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("'staging' is not configured", result.stderr)

    def test_local_uses_the_docker_broker(self):
        result, calls = self.run_script(MIGRATION, '-n', '1.0.1')
        self.assertEqual(result.returncode, 0, result.stderr)
        creates = [c for c in calls if '--create' in c]
        self.assertTrue(creates)
        self.assertTrue(all(c.startswith('docker exec -i tno-broker kafka-topics') for c in creates))
        self.assertTrue(all('--bootstrap-server broker:29092 --partitions 1 --replication-factor 1' in c for c in creates))

    def test_topic_add_and_delete(self):
        result, calls = self.run_script(TOPIC_ADD, '-e', 'dev', '-t', 'example', '-p', '4', '-c', 'retention.ms=1000')
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertTrue(any('--topic example' in c and '--partitions 4' in c and '--config retention.ms=1000' in c for c in calls))

        result, calls = self.run_script(TOPIC_DELETE, '-e', 'dev', '-t', 'example', '-y', EXISTING='example')
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertTrue(any('--delete' in c and '--topic example' in c for c in calls))


if __name__ == '__main__':
    unittest.main()
