import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { Button, ButtonVariant, Col, IReportTemplateModel, Row } from 'tno-core';

import { defaultRazorTemplate } from './constants';

/**
 * The page used to view and edit a report template.
 * @returns Component.
 */
export const ReportTemplateFormTemplate: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportTemplateModel>();

  return (
    <>
      <h2>{values.name}</h2>
      <Row>
        <Col flex="1">
          <p>Editing this template will change all reports that use this template.</p>
        </Col>
        <Button
          variant={ButtonVariant.secondary}
          onClick={() => setFieldValue('body', defaultRazorTemplate)}
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
