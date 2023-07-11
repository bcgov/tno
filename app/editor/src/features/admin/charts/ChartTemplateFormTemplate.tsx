import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { Col, IChartTemplateModel } from 'tno-core';

/**
 * The page used to view and edit a chart template.
 * @returns Component.
 */
export const ChartTemplateFormTemplate: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IChartTemplateModel>();

  return (
    <>
      <h2>{values.name}</h2>
      <Col className="code frm-in">
        <label htmlFor="txa-template" className="required">
          Chart Template
        </label>
        <p>Editing this template will change all reports that use this template.</p>
        <Col className="editor">
          <Editor
            id="txa-template"
            required
            value={values.template}
            onValueChange={(code) => setFieldValue('template', code)}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
    </>
  );
};
