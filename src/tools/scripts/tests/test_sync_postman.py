"""Exercise generated request contracts without a running API or network calls."""

import importlib.util
import json
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location("sync_postman", Path(__file__).resolve().parents[1] / "sync-postman.py")
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


def requests(folder):
    for item in folder["item"]:
        if "item" in item:
            yield from requests(item)
        else:
            yield item["request"]


class PostmanTests(unittest.TestCase):
    def test_aliases_are_excluded_and_http_methods_preserved(self):
        doc = {"paths": {path: {"get": {}, "delete": {}} for path in ("/api/admin/items/{id}", "/api/v{version}/admin/items/{id}", "/admin/items/{id}")}}
        result = list(requests(module.api_folder(doc)))
        self.assertEqual({r["method"] for r in result}, {"GET", "DELETE"})
        self.assertEqual(len(result), 2)
        self.assertTrue(all(r["url"]["raw"].endswith("/admin/items/:id") for r in result))

    def test_parameters_respect_optional_and_required_binding(self):
        doc = {"paths": {"/api/admin/items/{id}": {"get": {"parameters": [
            {"name": "id", "in": "path", "required": True, "schema": {"type": "integer"}},
            {"name": "filter", "in": "query", "schema": {"type": "string"}},
            {"name": "count", "in": "query", "required": True, "schema": {"type": "integer", "default": "2"}},
        ]}}}}
        request = list(requests(module.api_folder(doc)))[0]
        self.assertEqual(request["url"]["variable"][0]["value"], "1")
        self.assertTrue(request["url"]["query"][0]["disabled"])
        self.assertTrue(request["url"]["raw"].endswith("?count=2"))

    def test_json_examples_omit_readonly_and_stop_recursive_models(self):
        doc = {"components": {"schemas": {"Model": {"type": "object", "properties": {
            "id": {"type": "integer", "readOnly": True},
            "name": {"type": "string"}, "parent": {"$ref": "#/components/schemas/Model"},
            "status": {"type": "string", "enum": ["Draft", "Published"]},
        }}}}, "paths": {"/api/admin/items": {"post": {"requestBody": {"content": {"application/json": {"schema": {"$ref": "#/components/schemas/Model"}}}}}}}}
        request = list(requests(module.api_folder(doc)))[0]
        self.assertEqual(json.loads(request["body"]["raw"]), {"name": "string", "status": "Draft"})
        self.assertNotIn("auth", request)  # Inherits collection Bearer auth.

    def test_multipart_upload_uses_a_file_selector_and_generated_boundary(self):
        doc = {"paths": {"/api/editor/storage/upload": {"post": {"requestBody": {"content": {"multipart/form-data": {"schema": {"type": "object", "properties": {"files": {"type": "array", "items": {"type": "string", "format": "binary"}}}}}}}}}}}
        request = list(requests(module.api_folder(doc)))[0]
        self.assertEqual(request["body"]["formdata"][0]["type"], "file")
        self.assertEqual(request["header"], [])

    def test_public_health_does_not_send_a_bearer_token(self):
        doc = {"paths": {"/api/Health/live": {"get": {}}}}
        self.assertEqual(list(requests(module.api_folder(doc)))[0]["auth"], {"type": "noauth"})


if __name__ == "__main__":
    unittest.main()
