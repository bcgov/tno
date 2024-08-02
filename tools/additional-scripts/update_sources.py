# This script updates MMI Editor sources.
# Please only use this script to update sources to DEV or TEST site.

# The input data is in json format file named 'to_import_sources.json'
# The data can be from Prod sources.

import requests
import json

# copy token and replace <token> below
api_token = '<token>'
# TEST site url https://test.editor.mmi.gov.bc.ca/api/admin/sources
# DEV site url https://dev.editor.mmi.gov.bc.ca/api/admin/sources/
base_url = 'https://dev.editor.mmi.gov.bc.ca/api/admin/sources/'
api_header = {'Authorization': api_token}
response = requests.get(base_url, headers=api_header)

sources_array = json.loads(response.text)

file_name='to_import_sources.json'
to_import_file = open(file_name)
to_import_array = json.load(to_import_file)


for to_import_item in to_import_array:
    code = to_import_item['code']
    to_update = next((x for x in sources_array if x['code'] == code), None)
    if to_update is not None:
        url = base_url + str(to_update['id'])
    
        has_changes = False
        #licenseId
        #license
        if to_update['licenseId'] != to_import_item['licenseId']:
            if has_changes is False:
                has_changes = True
            to_update['licenseId'] = to_import_item['licenseId']
            to_update['license'] = to_import_item['license']
        #mediaTypeId
        #mediaType
        if to_update['mediaTypeId'] != to_import_item['mediaTypeId']:
            if has_changes is False:
                has_changes = True
            to_update['mediaTypeId'] = to_import_item['mediaTypeId']
            to_update['mediaType'] = to_import_item['mediaType']
        #mediaTypeSearchMappings
        if to_update['mediaTypeSearchMappings'] != to_import_item['mediaTypeSearchMappings']:        
            names_to_update = [x['name'] for x in json.loads(json.dumps(to_update['mediaTypeSearchMappings']))]
            names_to_import = [x['name'] for x in json.loads(json.dumps(to_import_item['mediaTypeSearchMappings']))]
            if sorted(names_to_update) != sorted(names_to_import):
                if has_changes is False:
                    has_changes = True
                to_update['mediaTypeSearchMappings'] = to_import_item['mediaTypeSearchMappings']
        #sortOrder
        if to_update['sortOrder'] != to_import_item['sortOrder']:
            if has_changes is False:
                has_changes = True
            to_update['sortOrder'] = to_import_item['sortOrder']
        #name
        if to_update['name'] != to_import_item['name']:
            if has_changes is False:
                has_changes = True
            to_update['name'] = to_import_item['name']

        if has_changes is True:
            print('update source by code: ' + code)
            response = requests.put(url, json=to_update, headers=api_header)
            print(response)
    else:
        print('The code "' + code + '" does not exists, skipped the update.')
