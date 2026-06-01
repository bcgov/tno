import unittest

from detect_deploy_changes import detect_deploy_changes


class DetectDeployChangesTests(unittest.TestCase):
    def test_all_scope_selects_everything(self):
        result = detect_deploy_changes([], "all")

        self.assertTrue(result["has_builds"])
        self.assertTrue(result["deploy_api"])
        self.assertTrue(result["deploy_services"])
        self.assertTrue(result["deploy_frontend"])
        self.assertTrue(result["run_db_migration"])
        self.assertEqual(len(result["build_matrix"]["include"]), 21)
        self.assertEqual(len(result["service_matrix"]["include"]), 18)
        self.assertEqual(len(result["frontend_matrix"]["include"]), 2)

    def test_editor_change_only_builds_and_deploys_editor(self):
        result = detect_deploy_changes(["app/editor/src/App.tsx"], "changed")

        self.assertEqual(
            result["build_matrix"]["include"],
            [
                {
                    "name": "editor",
                    "image": "editor",
                    "dockerfile": "app/editor/Dockerfile.nginx",
                    "context": "./app/editor",
                    "build_arg_name": "REACT_APP_VERSION",
                }
            ],
        )
        self.assertEqual(result["frontend_matrix"]["include"], [{"name": "editor", "deployment": "editor"}])
        self.assertFalse(result["deploy_api"])
        self.assertFalse(result["deploy_services"])
        self.assertFalse(result["run_db_migration"])

    def test_shared_dotnet_change_builds_api_and_dotnet_services(self):
        result = detect_deploy_changes(["libs/net/entities/Models/Content.cs"], "changed")
        build_names = {item["name"] for item in result["build_matrix"]["include"]}
        service_names = {item["name"] for item in result["service_matrix"]["include"]}

        self.assertIn("api", build_names)
        self.assertIn("content-service", build_names)
        self.assertIn("folder-collection-service", build_names)
        self.assertNotIn("charts-api", build_names)
        self.assertNotIn("editor", build_names)
        self.assertTrue(result["deploy_api"])
        self.assertTrue(result["deploy_services"])
        self.assertIn("content-service", service_names)
        self.assertFalse(result["run_db_migration"])

    def test_dal_change_runs_db_migration(self):
        result = detect_deploy_changes(["libs/net/dal/Migrations/20260512034029_1.5.0.cs"], "changed")

        self.assertTrue(result["deploy_api"])
        self.assertTrue(result["deploy_services"])
        self.assertTrue(result["run_db_migration"])

    def test_libs_net_dockerfile_change_does_not_run_db_migration(self):
        result = detect_deploy_changes(["libs/net/Dockerfile"], "changed")

        self.assertTrue(result["deploy_api"])
        self.assertTrue(result["deploy_services"])
        self.assertFalse(result["run_db_migration"])

    def test_service_change_only_selects_that_service(self):
        result = detect_deploy_changes(["services/net/indexing/IndexingService.cs"], "changed")

        self.assertEqual(result["build_matrix"]["include"][0]["name"], "indexing-service")
        self.assertEqual(result["service_matrix"]["include"], [{"name": "indexing-service", "deployment": "indexing-service"}])
        self.assertFalse(result["deploy_api"])
        self.assertTrue(result["deploy_services"])
        self.assertFalse(result["deploy_frontend"])

    def test_no_relevant_changes_has_empty_matrices(self):
        result = detect_deploy_changes(["docs/readme.md"], "changed")

        self.assertFalse(result["has_builds"])
        self.assertFalse(result["deploy_api"])
        self.assertFalse(result["deploy_services"])
        self.assertFalse(result["deploy_frontend"])
        self.assertFalse(result["run_db_migration"])
        self.assertEqual(result["build_matrix"], {"include": []})


if __name__ == "__main__":
    unittest.main()
