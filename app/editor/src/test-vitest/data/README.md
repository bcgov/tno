# Test Data Directory

This directory contains various data files required for testing, organized by functional modules.

## Directory Structure for example 

```
data/
├── content/                 # Content-related test data
│   ├── valid_items.json     # Valid content items
│   └── invalid_items.json   # Invalid content items
├── user/                    # User-related test data
│   └── profiles.json        # User profiles
│   
└── api/                     # API response samples
    └── response_samples/
        ├── content_200.json # Content API success response
        └── auth_401.json    # Authentication failure response
```
