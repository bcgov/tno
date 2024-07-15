import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useFilters, useFolders, useReportTemplates } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import { Button, ButtonVariant, Col, IReportModel, Row, Show } from 'tno-core';

import { exportReport, parseExportedReport } from './utils';

/**
 * The page used to import and export report.
 * @returns Component.
 */
export const ReportFormImportExport: React.FC = () => {
  const [{ userInfo }] = useApp();
  const { values, setValues } = useFormikContext<IReportModel>();
  const [{ reportTemplates }] = useAdminStore();
  const [, { findAllReportTemplates }] = useReportTemplates();
  const [{ filters }, { findFilters }] = useFilters();
  const [{ folders }, { findFolders }] = useFolders();
  const [rawReport, setRawReport] = React.useState('{}');

  const parseImport = React.useCallback(
    (data: string) => {
      try {
        const rawExportedReport = JSON.parse(data);

        var importedReport = parseExportedReport(
          rawExportedReport,
          reportTemplates,
          filters,
          folders,
        );
        // only for debugging
        // setRawReport(JSON.stringify(importedReport));
        setValues({
          ...importedReport,
          ownerId: userInfo?.id ?? 0,
        });
      } catch (ex) {
        const error = ex as Error;
        console.error(ex);
        toast.error(`Report Import Failed - ${error.message}`);
      }
    },
    [setValues, userInfo, reportTemplates, filters, folders],
  );

  React.useEffect(() => {
    if (!reportTemplates.length) findAllReportTemplates();
    if (!filters.length) findFilters({});
    if (!folders.length) findFolders({});
    // Only fetch items on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <>
      <Col>
        <h2>{values.name}</h2>
        <Row>
          <Col flex="1">
            <p>Import / Export.</p>
          </Col>
          <Show visible={values?.id === 0}>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => {
                parseImport(rawReport);
              }}
            >
              Import Report
            </Button>
          </Show>
          <Show visible={values?.id > 0}>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => {
                var exportedReport = exportReport(
                  values.name,
                  values.description,
                  values.isEnabled,
                  values.isPublic,
                  values.settings,
                  values.sections,
                  values.template,
                );
                setRawReport(JSON.stringify(exportedReport));
              }}
            >
              Export Report
            </Button>
          </Show>
        </Row>
      </Col>
      <Col className="code frm-importexportreport">
        <label htmlFor="txa-importexportreport">Raw report</label>
        <p>Paste your exported report below and an attempt will be made to parse it.</p>
        <Col className="editor">
          <Editor
            id="txa-importexportreport"
            value={rawReport}
            onValueChange={(importJson) => {
              try {
                // parseImport(JSON.parse(importJson));
                setRawReport(importJson);
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
