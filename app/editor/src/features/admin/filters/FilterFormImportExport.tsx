import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { toast } from 'react-toastify';
import { useLookupOptions } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  generateQuery,
  IFilterModel,
  IFilterSettingsModel,
  Row,
  Show,
} from 'tno-core';

import { exportFilter, parseExportedFilter } from './utils';

/**
 * The page used to view and edit report filter.
 * @returns Component.
 */
export const FilterFormImportExport: React.FC = () => {
  const { values, setFieldValue, setValues } = useFormikContext<IFilterModel>();
  const [{ series, products, sources, contributors, actions }] = useLookupOptions();

  const [rawFilter, setRawFilter] = React.useState('{}');

  const parseImport = React.useCallback(
    (data: string) => {
      try {
        const rawExportedFilter = JSON.parse(data);

        var importedFilter = parseExportedFilter(
          rawExportedFilter,
          actions,
          contributors,
          series,
          sources,
          products,
        );
        setValues({ ...importedFilter });

        const query = generateQuery(importedFilter.settings as IFilterSettingsModel, null);
        setFieldValue('query', query);
      } catch (ex) {
        const error = ex as Error;
        console.error(ex);
        toast.error(error.message);
      }
    },
    [setValues, setFieldValue, actions, contributors, series, sources, products],
  );

  return (
    <>
      <Col>
        <h2>{values.name}</h2>
        <Row>
          <Col flex="1">
            <p>Import / Export.</p>
          </Col>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              parseImport(rawFilter);
            }}
          >
            Import Filter
          </Button>
          <Show visible={values?.id > 0}>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => {
                var exportedFilter = exportFilter(
                  values.name,
                  values.description,
                  values.settings,
                  actions,
                  contributors,
                  series,
                  sources,
                  products,
                );
                setRawFilter(JSON.stringify(exportedFilter));
              }}
            >
              Export Filter
            </Button>
          </Show>
        </Row>
      </Col>
      <Col className="code frm-importexportfilter">
        <label htmlFor="txa-importexportfilter">Raw filter</label>
        <p>Paste your exported filter below and an attempt will be made to parse it.</p>
        <Col className="editor">
          <Editor
            id="txa-importexportfilter"
            value={rawFilter}
            onValueChange={(importJson) => {
              try {
                // parseImport(JSON.parse(importJson));
                setRawFilter(importJson);
              } catch {
                // Ignore errors.
                // TODO: Inform user of formatting issues on blur/validation.
              }
            }}
            highlight={(code) => {
              return highlight(code, languages.json, 'json');
            }}
          />
        </Col>
      </Col>
    </>
  );
};
