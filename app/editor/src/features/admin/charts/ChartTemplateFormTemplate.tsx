import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import {
  Button,
  ButtonVariant,
  chartTypeOptions,
  Col,
  datasetOptions,
  datasetValueOptions,
  FormikSelect,
  groupByOptions,
  OptionItem,
  Row,
} from 'tno-core';

import { useChartTemplateContext } from './ChartTemplateContext';
import {
  defaultChartTemplate,
  defaultCountRazorTemplate,
  defaultDateRazorTemplate,
} from './constants';

export interface IChartTemplateFormTemplateProps {}

/**
 * The page used to view and edit a chart template.
 * @returns Component.
 */
export const ChartTemplateFormTemplate: React.FC<IChartTemplateFormTemplateProps> = () => {
  const { values, setFieldValue } = useChartTemplateContext();

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
          label="Supported Datasets"
          name="settings.dataset"
          options={datasetOptions}
          isMulti
          value={
            values.settings?.dataset?.map((ct) => datasetOptions.find((o) => o.value === ct)) ?? []
          }
          onChange={(newValue) => {
            const values = newValue as OptionItem[];
            setFieldValue(
              'settings.dataset',
              values.map((o) => o.value),
            );
          }}
        />
        <FormikSelect
          label="Supported Dataset Value"
          name="settings.datasetValue"
          options={datasetValueOptions}
          isMulti
          value={
            values.settings?.datasetValue?.map((ct) =>
              datasetValueOptions.find((o) => o.value === ct),
            ) ?? []
          }
          onChange={(newValue) => {
            const values = newValue as OptionItem[];
            setFieldValue(
              'settings.datasetValue',
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
            }}
          >
            Count Template
          </Button>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFieldValue('template', defaultDateRazorTemplate);
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
