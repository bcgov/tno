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
  IChartTemplateModel,
  OptionItem,
  Row,
} from 'tno-core';

import {
  chartTypeOptions,
  defaultChartTemplate,
  defaultCountRazorTemplate,
  defaultDateRazorTemplate,
  groupByOptions,
} from './constants';
import { IChartPreviewRequestForm } from './interfaces';

export interface IChartTemplateFormTemplateProps {
  setPreview: React.Dispatch<React.SetStateAction<IChartPreviewRequestForm>>;
}

/**
 * The page used to view and edit a chart template.
 * @returns Component.
 */
export const ChartTemplateFormTemplate: React.FC<IChartTemplateFormTemplateProps> = ({
  setPreview,
}) => {
  const { values, setFieldValue } = useFormikContext<IChartTemplateModel>();

  React.useEffect(() => {
    // Initializes settings if the DB has no value.
    if (!values.settings) {
      setFieldValue('settings', defaultChartTemplate.settings);
    }
  }, [setFieldValue, values.settings]);

  return (
    <>
      <h2>{values.name}</h2>
      <Col>
        <FormikSelect
          label="Supported Chart Types"
          name="settings.chartTypes"
          options={chartTypeOptions}
          isMulti
          value={
            values.settings?.chartTypes?.map((ct) =>
              chartTypeOptions.find((o) => o.value === ct),
            ) ?? []
          }
          onChange={(newValue) => {
            const values = newValue as OptionItem[];
            setFieldValue(
              'settings.chartTypes',
              values.map((o) => o.value),
            );
          }}
        />
        <FormikSelect
          label="Supported Group By Options"
          name="settings.groupBy"
          options={groupByOptions}
          isMulti
          value={
            values.settings?.groupBy?.map((ct) => groupByOptions.find((o) => o.value === ct)) ?? []
          }
          onChange={(newValue) => {
            const values = newValue as OptionItem[];
            setFieldValue(
              'settings.groupBy',
              values.map((o) => o.value),
            );
          }}
        />
      </Col>
      <hr />
      <Col className="code frm-in">
        <Row>
          <Col flex="1">
            <label htmlFor="txa-template" className="required">
              Chart Template
            </label>
            <p>Editing this template will change all reports that use this template.</p>
          </Col>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFieldValue('template', defaultCountRazorTemplate);
              setPreview((preview) => ({ ...preview, template: defaultCountRazorTemplate }));
            }}
          >
            Count Template
          </Button>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFieldValue('template', defaultDateRazorTemplate);
              setPreview((preview) => ({ ...preview, template: defaultDateRazorTemplate }));
            }}
          >
            Date Template
          </Button>
        </Row>
        <Col className="editor">
          <Editor
            id="txa-template"
            required
            value={values.template}
            onValueChange={(code) => {
              setFieldValue('template', code);
              setPreview((preview) => ({ ...preview, template: code }));
            }}
            highlight={(code) => {
              return highlight(code, languages.cshtml, 'razor');
            }}
          />
        </Col>
      </Col>
    </>
  );
};
