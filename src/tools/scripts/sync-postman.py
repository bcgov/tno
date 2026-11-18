#!/usr/bin/env python3
"""Refresh the Postman API folder from the current API's exported OpenAPI document."""

import argparse
import json
from pathlib import Path
import re
from urllib.parse import quote

METHODS = {"get", "post", "put", "patch", "delete", "head", "options"}
BASE = "{{scheme}}://{{host}}:{{port}}{{root-path}}"


def resolve(schema, document):
    """Resolve local schema references without following recursive properties."""
    if "$ref" not in schema:
        return schema
    value = document
    for part in schema["$ref"].removeprefix("#/").split("/"):
        value = value[part.replace("~1", "/").replace("~0", "~")]
    return {**value, **{k: v for k, v in schema.items() if k != "$ref"}}


def example(schema, document, seen=()):
    """Create editable, type-correct examples; never reuse production response data."""
    ref = schema.get("$ref")
    if ref and ref in seen:
        return None
    seen = (*seen, ref) if ref else seen
    schema = resolve(schema, document)
    if "example" in schema:
        return schema["example"]
    if "default" in schema:
        value = schema["default"]
        if schema.get("type") == "integer":
            return int(value)
        if schema.get("type") == "number":
            return float(value)
        if schema.get("type") == "boolean" and isinstance(value, str):
            return value.lower() == "true"
        return value
    if schema.get("enum"):
        return schema["enum"][0]
    if "allOf" in schema:
        merged = {}
        for child in schema["allOf"]:
            value = example(child, document, seen)
            if isinstance(value, dict):
                merged.update(value)
        return merged
    for union in ("oneOf", "anyOf"):
        if schema.get(union):
            return example(schema[union][0], document, seen)
    kind = schema.get("type")
    if kind == "object" or "properties" in schema:
        result = {}
        for name, child in schema.get("properties", {}).items():
            if resolve(child, document).get("readOnly"):
                continue
            value = example(child, document, seen)
            if value is not None:
                result[name] = value
        return result
    if kind == "array":
        # Avoid enormous navigation-property graphs and recursive model examples.
        return []
    if kind in ("integer", "number"):
        return max(1, schema.get("minimum", 1))
    if kind == "boolean":
        return False
    return {
        "date-time": "2026-01-01T00:00:00Z",
        "date": "2026-01-01",
        "uuid": "00000000-0000-0000-0000-000000000001",
        "email": "example@example.com",
        "uri": "https://example.com",
    }.get(schema.get("format"), "string")


def text_value(value):
    return value if isinstance(value, str) else json.dumps(value, separators=(",", ":"))


def api_folder(document):
    groups = {}
    for path, path_item in sorted(document["paths"].items()):
        # Controllers expose several aliases; retain one unversioned /api route.
        if not path.startswith("/api/") or re.match(r"/api/v(?:\d|\{)", path):
            continue
        relative = path[len("/api"):]
        for method, operation in sorted(path_item.items()):
            if method not in METHODS:
                continue
            route = re.sub(r"\{([^}:]+)(?::[^}]+)?\}", r":\1", relative)
            url = {"raw": BASE + route, "protocol": "{{scheme}}", "host": ["{{host}}"], "port": "{{port}}", "path": ["{{root-path}}", *route.lstrip("/").split("/")]}
            request = {"method": method.upper(), "header": [], "url": url}
            descriptions = [operation.get("summary", ""), operation.get("description", "")]
            descriptions.append("Generated from the current API OpenAPI definition. Replace example IDs and body values before sending. This request may change application data.")
            for parameter in [*path_item.get("parameters", []), *operation.get("parameters", [])]:
                parameter = resolve(parameter, document)
                schema = parameter.get("schema", {})
                value = text_value(example(schema, document))
                field = {"key": parameter["name"], "value": value}
                if parameter.get("description"):
                    field["description"] = parameter["description"]
                location = parameter["in"]
                if location == "path":
                    url.setdefault("variable", []).append(field)
                elif location == "query":
                    field["disabled"] = not parameter.get("required", False)
                    url.setdefault("query", []).append(field)
                elif location == "header":
                    field["disabled"] = not parameter.get("required", False) and parameter["name"] != "api-version"
                    request["header"].append(field)
            enabled = [q for q in url.get("query", []) if not q["disabled"]]
            if enabled:
                url["raw"] += "?" + "&".join(quote(q["key"]) + "=" + quote(q["value"]) for q in enabled)
            body = resolve(operation.get("requestBody", {}), document)
            content = body.get("content", {})
            if "application/json" in content:
                schema = content["application/json"].get("schema", {})
                request["header"].append({"key": "Content-Type", "value": "application/json"})
                request["body"] = {"mode": "raw", "raw": json.dumps(example(schema, document), indent=2), "options": {"raw": {"language": "json"}}}
                descriptions.append("Body schema: " + schema.get("$ref", "inline"))
            elif "multipart/form-data" in content:
                schema = resolve(content["multipart/form-data"]["schema"], document)
                fields = []
                for key, child in schema.get("properties", {}).items():
                    child = resolve(child, document)
                    file_schema = resolve(child.get("items", child), document)
                    if file_schema.get("format") == "binary":
                        fields.append({"key": key, "type": "file", "src": [], "description": "Select local file(s) before sending."})
                    else:
                        fields.append({"key": key, "type": "text", "value": text_value(example(child, document))})
                request["body"] = {"mode": "formdata", "formdata": fields}
            if path.lower().startswith("/api/health"):
                request["auth"] = {"type": "noauth"}
            request["description"] = "\n\n".join(d for d in descriptions if d)
            parts = relative.strip("/").split("/")
            area = parts[0].lower()
            resource = parts[1] if area in {"admin", "editor", "subscriber", "services"} and len(parts) > 1 else area
            groups.setdefault(area, {}).setdefault(resource, []).append({"name": method.upper() + " " + relative, "request": request, "response": []})
    folders = [{"name": area, "item": [{"name": resource, "item": items} for resource, items in sorted(resources.items())]} for area, resources in sorted(groups.items())]
    return {"name": "api", "description": "Current .NET API routes, generated from OpenAPI. Uses collection Bearer authentication except public health endpoints. Optional query parameters are disabled until selected. Examples are templates, not seeded records. Request.Query filters implemented outside model binding may not appear in OpenAPI; consult the controller for additional filters.", "item": folders}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--openapi", type=Path, required=True)
    parser.add_argument("--collection", type=Path, default=Path(__file__).resolve().parents[2] / "api/TNO.postman_collection.json")
    parser.add_argument("--check", action="store_true", help="Check for drift without writing")
    args = parser.parse_args()
    document = json.loads(args.openapi.read_text())
    collection = json.loads(args.collection.read_text())
    generated = api_folder(document)
    assert generated["item"], "OpenAPI has no canonical /api routes"
    for index, folder in enumerate(collection["item"]):
        if folder["name"] == "api":
            if args.check:
                if folder != generated:
                    raise SystemExit("Postman API folder is out of date; rerun without --check.")
                print("Postman API folder matches OpenAPI.")
                return
            collection["item"][index] = generated
            break
    else:
        raise SystemExit("Collection has no api folder")
    args.collection.write_text(json.dumps(collection, indent=2, ensure_ascii=False) + "\n")
    print(f"Updated {args.collection}")


if __name__ == "__main__":
    main()
