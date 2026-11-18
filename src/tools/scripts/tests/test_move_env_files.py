"""Regression tests for migrating ignored developer environment files."""

import importlib.util
from pathlib import Path
import stat
import shutil
import subprocess
import tempfile
import unittest

spec = importlib.util.spec_from_file_location(
    "move_env_files", Path(__file__).resolve().parents[1] / "move-env-files.py"
)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class MoveEnvironmentTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def write(self, path, content=b"PRIVATE=value\r\n"):
        file = self.root / path
        file.parent.mkdir(parents=True, exist_ok=True)
        file.write_bytes(content)
        file.chmod(0o600)
        return file

    def test_make_move_uses_the_repository_root(self):
        repo = Path(__file__).resolve().parents[4]
        script = self.root / "src/tools/scripts/move-env-files.py"
        script.parent.mkdir(parents=True)
        shutil.copyfile(repo / "src/tools/scripts/move-env-files.py", script)
        shutil.copyfile(repo / "Makefile", self.root / "Makefile")
        self.write("api/net/.env")
        result = subprocess.run(
            ["make", "move"], cwd=self.root, capture_output=True, text=True
        )
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertFalse((self.root / "api/net/.env").exists())
        self.assertEqual((self.root / "src/api/.env").read_bytes(), b"PRIVATE=value\r\n")

    def test_all_locations_variants_permissions_and_repeat_run(self):
        paths = {
            "api/net/.env": "src/api/.env",
            "playwright-automation/.env": "src/playwright-automation/.env",
            "app/.env.local": "src/app/.env.local",
            "libs/net/dal/.env": "src/libs/dal/.env",
            "services/custom service/local.env": "src/services/custom service/local.env",
            "tools/indexer/.env": "src/tools/indexer/.env",
            "db/postgres/docker/.env": "dependencies/postgres/docker/.env",
            "auth/keycloak/.env": "dependencies/keycloak/.env",
            "network/nginx/.env": "dependencies/nginx/.env",
        }
        root_env = self.write(".env")
        ignored = self.write("app/node_modules/package/.env")
        unrelated = self.write("api/net/notes.txt")
        for old in paths:
            self.write(old)
        self.assertEqual(module.migrate(self.root), len(paths))
        for old, new in paths.items():
            self.assertFalse((self.root / old).exists())
            self.assertEqual((self.root / new).read_bytes(), b"PRIVATE=value\r\n")
            self.assertEqual(stat.S_IMODE((self.root / new).stat().st_mode), 0o600)
        self.assertEqual(root_env.read_bytes(), b"PRIVATE=value\r\n")
        self.assertTrue(ignored.exists())
        self.assertTrue(unrelated.exists())
        self.assertEqual(module.migrate(self.root), 0)

    def test_conflict_does_not_partially_move_or_overwrite(self):
        source = self.write("api/net/.env")
        conflict = self.write("network/nginx/.env")
        destination = self.write("dependencies/nginx/.env", b"KEEP=existing\n")
        with self.assertRaises(ValueError):
            module.migrate(self.root)
        self.assertTrue(source.exists())
        self.assertTrue(conflict.exists())
        self.assertEqual(destination.read_bytes(), b"KEEP=existing\n")

    def test_identical_destination_retains_both_files(self):
        old = self.write("app/.env")
        new = self.write("src/app/.env")
        self.assertEqual(module.migrate(self.root), 0)
        self.assertEqual(old.read_bytes(), new.read_bytes())

    def test_flattened_dependency_collision_is_rejected(self):
        self.write("db/shared/.env")
        self.write("auth/shared/.env")
        with self.assertRaises(ValueError):
            module.migrate(self.root)
        self.assertTrue((self.root / "db/shared/.env").exists())

    def test_flattened_environments_move_from_each_previous_layout(self):
        for source, destination in (
            ("api/node", "src/charts-api"),
            ("src/api/node", "src/charts-api"),
            ("api/net", "src/api"),
            ("src/api/net", "src/api"),
            ("libs/net/tests/api", "src/tests/api"),
            ("src/libs/net/tests/api", "src/tests/api"),
            ("src/libs/tests/api", "src/tests/api"),
            ("libs/net/dal", "src/libs/dal"),
            ("src/libs/net/dal", "src/libs/dal"),
            ("services/net/content", "src/services/content"),
            ("src/services/net/content", "src/services/content"),
        ):
            with self.subTest(source=source):
                old = self.write(f"{source}/.env.local")
                self.assertEqual(module.migrate(self.root), 1)
                new = self.root / destination / ".env.local"
                self.assertFalse(old.exists())
                self.assertEqual(new.read_bytes(), b"PRIVATE=value\r\n")
                self.assertEqual(module.migrate(self.root), 0)
                new.unlink()

    def test_api_conflict_prevents_charts_environment_move(self):
        charts = self.write("src/api/node/.env")
        self.write("src/api/net/.env")
        destination = self.write("src/api/.env", b"KEEP=existing\n")
        with self.assertRaises(ValueError):
            module.migrate(self.root)
        self.assertTrue(charts.exists())
        self.assertEqual(destination.read_bytes(), b"KEEP=existing\n")

    def test_kafka_ui_moves_from_each_previous_layout(self):
        for source in ("db/kafka/kafka-ui", "dependencies/kafka/kafka-ui"):
            with self.subTest(source=source):
                old = self.write(f"{source}/.env.local")
                self.assertEqual(module.migrate(self.root), 1)
                new = self.root / "dependencies/kafka-ui/.env.local"
                self.assertFalse(old.exists())
                self.assertEqual(new.read_bytes(), b"PRIVATE=value\r\n")
                new.unlink()

    def test_kafka_ui_conflict_is_detected_across_previous_layouts(self):
        self.write("db/kafka/kafka-ui/.env")
        self.write("dependencies/kafka/kafka-ui/.env")
        with self.assertRaises(ValueError):
            module.migrate(self.root)
        self.assertTrue((self.root / "db/kafka/kafka-ui/.env").exists())

    def test_removed_npm_environments_are_not_migrated(self):
        old = self.write("libs/npm/old-package/.env")
        self.assertEqual(module.migrate(self.root), 0)
        self.assertTrue(old.exists())
        self.assertFalse((self.root / "src/libs/npm").exists())

    def test_obsolete_kowl_environments_are_not_migrated(self):
        old = self.write("db/kafka/kowl/.env")
        self.assertEqual(module.migrate(self.root), 0)
        self.assertTrue(old.exists())
        self.assertFalse((self.root / "dependencies/kafka/kowl").exists())

    def test_root_context_overrides_preserve_other_values(self):
        env = self.write(".env", b'DB_CONTEXT="./db/postgres/rhel8"\r\nORACLE_DB_CONTEXT=db/oracle/docker\r\nSECRET=db/unchanged\r\n')
        module.migrate(self.root)
        self.assertEqual(env.read_bytes(), b'DB_CONTEXT="./dependencies/postgres/rhel8"\r\nORACLE_DB_CONTEXT=dependencies/oracle/docker\r\nSECRET=db/unchanged\r\n')
        self.assertEqual(stat.S_IMODE(env.stat().st_mode), 0o600)


if __name__ == "__main__":
    unittest.main()
