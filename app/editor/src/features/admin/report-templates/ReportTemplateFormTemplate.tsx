import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import {
  Button,
  ButtonVariant,
  Col,
  FormikSelect,
  getEnumStringOptions,
  IOptionItem,
  IReportTemplateModel,
  ReportTypeName,
  Row,
} from 'tno-core';

import {
  defaultAVOverviewBodyRazorTemplate,
  defaultAVOverviewSubjectRazorTemplate,
  defaultContentBodyRazorTemplate,
  defaultContentSubjectRazorTemplate,
} from './constants';

/**
 * The page used to view and edit a report template.
 * @returns Component.
 */
export const ReportTemplateFormTemplate: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportTemplateModel>();

  const reportTypeOptions = getEnumStringOptions(ReportTypeName);

  return (
    <>
      <h2>{values.name}</h2>
      <p>Editing this template will change all reports that use this template.</p>
      <Row alignItems="flex-end">
        <FormikSelect
          name="reportType"
          label="Report Type"
          options={reportTypeOptions}
          tooltip="Each report type generates a different kind of report.  Choose the one this template is designed for."
          value={reportTypeOptions.filter((rt) => values.reportType === rt.value) ?? ''}
          isClearable={false}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            setFieldValue('reportType', option.value);
          }}
        />
        <Col flex="1"></Col>
        <Button
          variant={ButtonVariant.secondary}
          onClick={() => {
            setFieldValue(
              'subject',
              values.reportType === ReportTypeName.Content
                ? defaultContentSubjectRazorTemplate
                : defaultAVOverviewSubjectRazorTemplate,
            );
            setFieldValue(
              'body',
              values.reportType === ReportTypeName.Content
                ? defaultContentBodyRazorTemplate
                : defaultAVOverviewBodyRazorTemplate,
            );
          }}
        >
          Use Default Template
        </Button>
      </Row>
      <Col className="code frm-in">
        <label htmlFor="txa-subject" className="required">
          Subject Template
        </label>
        <Col className="editor">
          <Editor
            id="txa-subject-template"
            required
            value={values.subject}
            onValueChange={(code) => setFieldValue('subject', code)}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
      <Col className="code frm-in">
        <label htmlFor="txa-template" className="required">
          Report Template
        </label>
        <Col className="editor">
          <Editor
            id="txa-body-template"
            required
            value={values.body}
            onValueChange={(code) => setFieldValue('body', code)}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
    </>
  );
};
