"""Command contract tests: no container daemon or cluster is contacted."""
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[4]


class DoCommands(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='tno do ')
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        shutil.copy(ROOT / 'do', self.root)
        shutil.copytree(ROOT / 'src/tools/scripts/do', self.root / 'src/tools/scripts/do')
        scripts = self.root / 'src/tools/scripts'
        shutil.copy(ROOT / 'src/tools/scripts/confirm.sh', scripts)
        self.bin = self.root / 'bin'
        self.bin.mkdir()
        (self.root / 'src/libs/dal').mkdir(parents=True)
        (self.root / 'src/libs/dal/TNO.DAL.csproj').touch()
        self.log = self.root / 'calls'
        self.env = dict(os.environ, PATH=str(self.bin) + os.pathsep + os.environ['PATH'],
                        DO_CALLS=str(self.log))
        for name in ('docker', 'docker-compose', 'podman', 'oc', 'dotnet'):
            self.stub(self.bin / name)
        for name in ('db-apply', 'elastic-update', 'db-drop', 'refresh-keycloak', 'nuke', 'switch-project'):
            self.stub(scripts / (name + '.sh'))
        for name in ('get-pod-name', 'get-logs', 'rsh', 'port-forward', 'database-connect',
                     'release', 'deploy', 'stop', 'up', 'annotate', 'build', 'push', 'pull', 'tag'):
            self.stub(self.root / 'openshift/scripts' / (name + '.sh'))
        for name in ('migration', 'topic-add', 'topic-delete', 'reset-consumer-offset'):
            self.stub(self.root / 'dependencies/kafka/scripts' / (name + '.sh'))

    def stub(self, path):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text('''#!/usr/bin/env bash
python3 - "$0" "$@" <<'PY'
import json, os, sys
from pathlib import Path
name = Path(sys.argv[1]).name
args = sys.argv[2:]
with open(os.environ['DO_CALLS'], 'a') as f:
    f.write(json.dumps([name, args, os.getcwd(), os.environ.get('CONTAINER_ENGINE'),
                       [os.environ.get(k) for k in ('MIGRATE', 'DRY_RUN', 'YES')]]) + '\\n')
if os.environ.get('FAIL_COMMAND') in args or (os.environ.get('NO_V2') and args == ['compose', 'version']):
    sys.exit(7)
if name == 'dotnet' and args[:3] == ['ef', 'migrations', 'add']:
    migration = Path('Migrations') / ('20261119000000_' + args[3] + '.cs')
    migration.parent.mkdir(exist_ok=True)
    migration.write_text('\\ufeffusing Microsoft.EntityFrameworkCore.Migrations;\\n'
                        'namespace TNO.DAL.Migrations;\\n'
                        'public partial class Test : Migration\\n{\\n'
                        '    protected override void Up(MigrationBuilder migrationBuilder)\\n    {\\n'
                        '        migrationBuilder.Sql("SELECT \\"{\\";");\\n    }\\n'
                        '    protected override void Down(MigrationBuilder migrationBuilder)\\n    {\\n    }\\n}\\n', encoding='utf-8')
    migration.write_bytes(migration.read_bytes().replace(b'\\r\\n', b'\\n').replace(b'\\n', b'\\r\\n'))
PY
''')
        path.chmod(0o755)

    def run_do(self, *args, success=True, **env):
        self.log.unlink(missing_ok=True)
        result = subprocess.run([os.environ.get('DO_TEST_BASH', 'bash'), (self.root / 'do').as_posix(), *args], cwd=self.bin,
                                env={**self.env, **env}, input='', capture_output=True, text=True)
        self.assertEqual(result.returncode == 0, success, result.stdout + result.stderr)
        return [json.loads(line) for line in self.log.read_text().splitlines()] if self.log.exists() else []

    def test_help_and_invalid_commands_never_call_tools(self):
        for args in [(), ('docker', 'help'), ('podman',), ('oc', '--help')]:
            self.assertEqual(self.run_do(*args), [])
        for args in [('up',), ('docker', 'bad'), ('oc', 'bad'), ('docker', 'down', '-n', 'api'),
                     ('docker', 'up', 'bogus=1'), ('docker', 'up', '--bad')]:
            self.assertEqual(self.run_do(*args, success=False), [])

    def test_both_engines_use_all_compose_files_and_profiles(self):
        for engine in ('docker', 'podman'):
            for action in ('up', 'build', 'stop', 'remove', 'restart', 'refresh'):
                calls = self.run_do(engine, action, '-n', 'api', '-p', 'main')
                for name, args, cwd, runtime, _ in calls:
                    if args == ['compose', 'version']:
                        continue
                    self.assertEqual(name, engine)
                    self.assertEqual(runtime, engine)
                    self.assertTrue(Path(cwd).samefile(self.root))
                    self.assertEqual(args.count('-f'), 5 if 'rm' in args and action == 'refresh' else 4)
                    self.assertIn('src/services/docker-compose.yml', args)
                    self.assertEqual(args[args.index('--profile') + 1], 'main')
                    self.assertEqual(args[-1], 'api')

    def test_default_profiles_and_legacy_compose(self):
        args = self.run_do('docker', 'up')[-1][1]
        self.assertNotIn('--profile', args)
        self.assertIn('--env-file', args)
        args = self.run_do('podman', 'stop')[-1][1]
        self.assertEqual(args[-3:], ['--profile', 'all', 'stop'])
        calls = self.run_do('docker', 'up', '-n', 'api', NO_V2='1')
        self.assertEqual(calls[-1][0], 'docker-compose')
        self.assertIn('all', calls[-1][1])

    def test_deletion_confirmation_and_fail_fast(self):
        self.assertEqual(self.run_do('docker', 'down', '-v', success=False), [])
        self.assertEqual(self.run_do('podman', 'nuke', success=False), [])
        args = self.run_do('podman', 'down', '-v', '-y')[-1][1]
        self.assertEqual(args[-2:], ['down', '-v'])
        self.assertNotIn('-v', self.run_do('docker', 'down')[-1][1])
        calls = self.run_do('podman', 'refresh', '-n', 'api', FAIL_COMMAND='build', success=False)
        self.assertFalse(any('up' in c[1] for c in calls))

    def test_local_workflows(self):
        for engine in ('docker', 'podman'):
            for action in ('db-apply', 'db-refresh', 'db-nuke', 'elastic-update',
                           'refresh-keycloak', 'renew', 'reset-consumer-offset', 'nuke'):
                options = ['-y'] if action == 'nuke' else []
                calls = self.run_do(engine, action, *options)
                self.assertTrue(calls)
                self.assertTrue(all(c[3] == engine for c in calls))

    def test_oc_argument_order_and_empty_slots(self):
        calls = self.run_do('oc', 'deploy', '-n', 'elastic-migration', '-m', '1.0.11', '-p', 'copy')
        self.assertEqual(calls[-1][1], ['dev', 'latest', 'elastic-migration', '1.0.11', '', 'copy'])
        self.assertTrue(Path(calls[-1][2]).samefile(self.root / 'openshift/scripts'))
        cases = {
            'build': ['api', 'latest', 'prod'], 'push': ['api', 'latest'],
            'pull': ['api', 'latest'], 'pod': ['api'], 'logs': ['api', '0'],
            'annotate': ['dev', 'api'], 'tag': ['api', 'latest', '', 'remote'],
        }
        for action, expected in cases.items():
            self.assertEqual(self.run_do('oc', action, '-n', 'api')[-1][1], expected)
        for action in ('stop', 'up'):
            self.assertEqual(self.run_do('oc', action)[-1][1], ['dev'])
        self.assertEqual(self.run_do('oc', 'db-connect')[-1][1], ['dev', '22222', '5432'])
        self.assertEqual(self.run_do('oc', 'rsh', '-l', 'app=api')[-1][1], ['app=api', '0'])
        self.assertEqual(self.run_do('oc', 'port-forward', '-n', 'app=api', '-l', '8080', '-r', '80')[-1][1],
                         ['app=api', '0', '8080', '80'])
        self.assertEqual(self.run_do('oc', 'switch-current', '-n', 'test')[-1][1], ['test'])
        for action in ('project', 'switch', 'clean', 'kafka-topics'):
            self.assertEqual(self.run_do('oc', action)[-1][0], 'oc')
        calls = self.run_do('oc', 'release', '-n', 'api app', '-d', '-m', '-y')
        self.assertEqual(calls[-1][1], ['dev', '', 'api', 'app'])
        self.assertEqual(calls[-1][4], ['1', '1', '1'])

    def test_kafka_option_mapping_and_literal_values(self):
        for group, env in [('docker', 'local'), ('podman', 'local'), ('oc', 'dev')]:
            calls = self.run_do(group, 'kafka-update', '-n', '1.0.1', '-r', '-f', '3', '-d', '-y')
            self.assertEqual(calls[-1][1], ['-e', env, '-d', '-n', '1.0.1', '-r', '--replication', '3', '-y'])
        literal = 'topic $(touch injected); *'
        calls = self.run_do('podman', 'kafka-topic-add', '-t', literal, '-p', '6', '-r', '3', '-c', 'a=b,c=d', '-i', '0')
        self.assertEqual(calls[-1][1], ['-e', 'local', '-t', literal, '-i', '0', '-p', '6', '-r', '3', '-c', 'a=b,c=d'])
        self.assertFalse((self.root / 'injected').exists())
        self.assertEqual(self.run_do('oc', 'kafka-topic-delete', '-t', 'test', '-y')[-1][1],
                         ['-e', 'dev', '-t', 'test', '-y'])

    def test_real_migration_helpers_use_selected_engine_and_propagate_failure(self):
        for name in ('elastic-update',):
            shutil.copy(ROOT / ('src/tools/scripts/' + name + '.sh'), self.root / ('src/tools/scripts/' + name + '.sh'))
            calls = self.run_do('podman', name)
            self.assertTrue(all(call[0] == 'podman' for call in calls))
            self.assertTrue(any('run' in call[1] for call in calls))
            calls = self.run_do('podman', name, FAIL_COMMAND='build', success=False)
            self.assertFalse(any('run' in call[1] for call in calls))
            self.run_do('podman', name, FAIL_COMMAND='run', success=False)

    def test_real_release_dry_run_handles_empty_and_multiple_component_lists(self):
        self.stub(self.bin / 'git')
        for name in ('release', 'acr-common', 'deploy'):
            shutil.copy(ROOT / ('openshift/scripts/' + name + '.sh'), self.root / ('openshift/scripts/' + name + '.sh'))
        for options in ([], ['-n', 'api app']):
            calls = self.run_do('oc', 'release', '-d', '-t', 'command-test', *options)
            self.assertFalse(any(c[0] == 'oc' or 'build' in c[1] or 'push' in c[1] for c in calls))

    def test_local_database_commands_and_argument_order(self):
        cases = [
            (['db-migrations'], [['migrations', 'list']]),
            (['db-migrations', '--offline'], [['migrations', 'list', '--no-connect']]),
            (['db-apply'], [['database', 'update']]),
            (['db-apply', '-n', '1.5.16'], [['database', 'update', '1.5.16']]),
            (['db-rollback', '-n', '0'], [['database', 'update', '0']]),
            (['db-remove'], [['migrations', 'remove', '--force']]),
            (['db-drop'], [['database', 'drop']]),
            (['db-drop', '-y'], [['database', 'drop', '--force']]),
            (['db-refresh'], [['database', 'update', '0'], ['database', 'update']]),
            (['db-clean'], [['database', 'drop', '--force'], ['database', 'update']]),
        ]
        for args, expected in cases:
            calls = self.run_do(*args)
            self.assertTrue(all(c[0] == 'dotnet' for c in calls))
            self.assertTrue(all(Path(c[2]).samefile(self.root / 'src/libs/dal') for c in calls if c[1] != ['ef', '--version']))
            self.assertEqual([c[1][1:] for c in calls if c[1][0] == 'ef' and c[1] != ['ef', '--version']], expected)
        calls = self.run_do('db-script', '--from', '1.5.14', '--to', '1.5.16', '--output', 'sql/my migration.sql')
        self.assertEqual(calls[-1][1][0:5], ['ef', 'migrations', 'script', '1.5.14', '1.5.16'])
        self.assertTrue(calls[-1][1][-1].endswith('/sql/my migration.sql'))

    def test_database_validation_and_failed_sequence(self):
        for prefix in ([], ['docker'], ['podman']):
            for args in (['db-add'], ['db-rollback'], ['db-script', '--output', '../outside.sql'],
                         ['db-add', '-n', '../bad'], ['db-script', '--output', r'..\outside.sql'],
                         ['db-apply', '-p', 'all'], ['db-unknown']):
                self.assertEqual(self.run_do(*prefix, *args, success=False), [])
        calls = self.run_do('db-clean', FAIL_COMMAND='drop', success=False)
        self.assertFalse(any('update' in c[1] for c in calls))
        calls = self.run_do('db-recreate', FAIL_COMMAND='remove', success=False)
        self.assertFalse(any('add' in c[1] for c in calls))

    def test_generated_seed_hooks_and_recreate_sequence(self):
        for command, version in [('db-add', '1.5.17'), ('db-recreate', '1.5.18')]:
            calls = self.run_do(command, '-n', version)
            migration = (self.root / ('src/libs/dal/Migrations/20261119000000_' + version + '.cs')).read_text(encoding='utf-8')
            self.assertIn(': SeedMigration', migration)
            self.assertNotIn('\ufeff', migration)
            for hook in ('PreUp', 'PostUp', 'PreDown', 'PostDown'):
                self.assertEqual(migration.count(hook + '(migrationBuilder);'), 1)
            self.assertLess(migration.index('PreUp('), migration.index('migrationBuilder.Sql'))
            self.assertLess(migration.index('migrationBuilder.Sql'), migration.index('PostUp('))
            if command == 'db-recreate':
                self.assertEqual([c[1] for c in calls if c[1][0] == 'ef'][1:], [
                    ['ef', 'database', 'update', '0'], ['ef', 'migrations', 'remove', '--force'],
                    ['ef', 'migrations', 'add', version], ['ef', 'database', 'update']])

    def test_container_database_commands_forward_options_and_mount_sources(self):
        for engine in ('docker', 'podman'):
            for command, options in [('db-migrations', ['--offline']), ('db-add', ['-n', '1.5.17']),
                    ('db-apply', ['-n', '1.5.16']), ('db-rollback', ['-n', '0']), ('db-remove', []),
                    ('db-drop', ['-y']), ('db-refresh', []), ('db-clean', []),
                    ('db-script', ['--from', '0', '--to', '1.5.16', '--output', 'sql/a b.sql']),
                    ('db-recreate', ['-r', '1.5.15', '-n', '1.5.17'])]:
                calls = self.run_do(engine, command, *options)
                self.assertEqual([c[0] for c in calls], [engine, engine])
                self.assertEqual(calls[0][1][0], 'build')
                args = calls[1][1]
                self.assertEqual(args[-(3 + len(options)):], ['bash', 'do', command, *options])
                self.assertIn('tno-net', args)
                self.assertIn('/workspace/src/libs/dal/obj:rw,mode=1777', args)
                self.assertTrue(any('type=bind,source=' in arg and 'target=/workspace' in arg for arg in args))
        calls = self.run_do('docker', 'db-migrations', '--offline', DB_NETWORK='none')
        self.assertNotIn('--network', calls[-1][1])

    def test_standard_flag_forms_and_repeated_values(self):
        args = self.run_do('docker', 'up', '-nold', '-napi', '-pmain')[-1][1]
        self.assertEqual(args[-1], 'api')
        self.assertEqual(args[args.index('--profile') + 1], 'main')
        self.assertEqual(self.run_do('podman', 'down', '-vy')[-1][1][-2:], ['down', '-v'])
        self.assertEqual(self.run_do('podman', 'down', '--volumes', '--yes')[-1][1][-2:], ['down', '-v'])
        calls = self.run_do('db-script', '--from=0', '--to=1.5.16', '--output=sql/a=b.sql')
        self.assertEqual(calls[-1][1][0:5], ['ef', 'migrations', 'script', '0', '1.5.16'])
        self.assertTrue(calls[-1][1][-1].endswith('/sql/a=b.sql'))
        self.assertTrue(self.run_do('db-script', '-osql/short.sql')[-1][1][-1].endswith('/sql/short.sql'))
        self.assertEqual(self.run_do('oc', 'release', '-mdy')[-1][4], ['1', '1', '1'])
        self.assertEqual(self.run_do('oc', 'deploy', '-m0')[-1][1][3], '0')
        self.assertEqual(self.run_do('docker', 'kafka-update', '-rdy', '-n1.0.1')[-1][1],
                         ['-e', 'local', '-d', '-n', '1.0.1', '-r', '-y'])
        self.assertIn('3', self.run_do('docker', 'kafka-topic-add', '-r3', '-tindex')[-1][1])
        self.assertIn('-d', self.run_do('oc', 'kafka-update', '--dry-run')[-1][1])

    def test_invalid_flags_fail_without_external_calls(self):
        cases = [
            ('docker', 'up', 'n=api'), ('db-script', 'from=0'),
            ('docker', 'up', '-n'), ('docker', 'up', '-n', ''),
            ('docker', 'up', '-n', '-p', 'main'), ('docker', 'up', '--unknown'),
            ('docker', 'up', '-z'), ('docker', 'up', '-napi', 'extra'),
            ('docker', 'up', '--', 'api'), ('docker', 'down', '-v', '0'),
            ('docker', 'down', '--volumes=false'), ('db-migrations', '--offline=0'),
            ('db-script', '--from'), ('db-script', '--from='),
            ('db-script', '--from', '--to', '1.5.16'), ('oc', 'release', '-m', '1'),
            ('docker', 'kafka-update', '-r', '1'), ('docker', 'up', '--offline'),
        ]
        for args in cases:
            self.assertEqual(self.run_do(*args, success=False), [], args)

    def test_help_after_every_command_group_has_no_side_effects(self):
        for flag in ('-h', '--help'):
            for args in [('docker', 'up'), ('podman', 'down'), ('oc', 'release'),
                         ('db-add',), ('db-script',), ('setup',), ('move',), ('worktree',)]:
                self.assertEqual(self.run_do(*args, flag), [], args)
