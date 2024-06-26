import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { Button, ButtonVariant, Checkbox, Col, mergeChartSettings, Row } from 'tno-core';

import { useChartTemplateContext } from './ChartTemplateContext';
import {
  defaultChartJSOptions,
  defaultChartJSStackedOptions,
  defaultChartTemplate,
} from './constants';

export interface IChartTemplateFormConfigProps {}

/**
 * The page used to view and edit a chart template Chart.js options.
 * @returns Component.
 */
export const ChartTemplateFormConfig: React.FC<IChartTemplateFormConfigProps> = () => {
  const { values, setFieldValue, setValues } = useChartTemplateContext();

  const [chartOptions, setChartOptions] = React.useState(
    JSON.stringify(values.settings?.options ?? '{}', null, 2),
  );

  React.useEffect(() => {
    // Initializes settings if the DB has no value.
    if (!values.settings) {
      setFieldValue('settings', defaultChartTemplate.settings);
    }
  }, [setFieldValue, values.settings]);

  return (
    <>
      <h2>{values.name}</h2>
      <Col className="code frm-in">
        <Row>
          <Col flex="1">
            <label htmlFor="txa-template" className="required">
              Chart JS Configuration
            </label>
            <p>
              Editing this configuration will provide the default for all reports that use this
              template.
            </p>
          </Col>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setChartOptions(JSON.stringify(defaultChartJSOptions, null, 2));
              setFieldValue('settings.options', defaultChartJSOptions);
            }}
          >
            Default
          </Button>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setChartOptions(JSON.stringify(defaultChartJSStackedOptions, null, 2));
              setFieldValue('settings.options', defaultChartJSStackedOptions);
            }}
          >
            Default Stacked
          </Button>
        </Row>
        <Row gap="1rem">
          <Checkbox
            name="horizontal"
            label="Flip X and Y axis"
            checked={values.settings.options?.indexAxis === 'y'}
            onChange={(e) => {
              var sectionSettings = mergeChartSettings(
                values.settings.options,
                values.sectionSettings,
                {
                  isHorizontal: e.target.checked,
                },
              );
              setChartOptions(JSON.stringify(sectionSettings.options, null, 2));
              setValues({
                ...values,
                settings: { ...values.settings, options: sectionSettings.options },
                sectionSettings,
              });
            }}
          />
          <Checkbox
            name="showLegend"
            label="Show Legend"
            checked={values.settings.options?.plugins?.legend?.display ?? ''}
            onChange={(e) => {
              var sectionSettings = mergeChartSettings(
                values.settings.options,
                values.sectionSettings,
                {
                  showLegend: e.target.checked,
                },
              );
              setChartOptions(JSON.stringify(sectionSettings.options, null, 2));
              setValues({
                ...values,
                settings: { ...values.settings, options: sectionSettings.options },
                sectionSettings,
              });
            }}
          />
          <Checkbox
            name="showDataValues"
            label="Show Data Labels"
            checked={values.settings.options?.plugins?.datalabels?.labels?.title?.display ?? ''}
            onChange={(e) => {
              var sectionSettings = mergeChartSettings(
                values.settings.options,
                values.sectionSettings,
                {
                  showDataLabels: e.target.checked,
                },
              );
              setChartOptions(JSON.stringify(sectionSettings.options, null, 2));
              setValues({
                ...values,
                settings: { ...values.settings, options: sectionSettings.options },
                sectionSettings,
              });
            }}
          />
        </Row>
        <Col className="editor">
          <Editor
            id="txa-settings-options"
            value={chartOptions}
            onValueChange={(code) => {
              setChartOptions(code);
              try {
                const json = JSON.parse(code);
                setFieldValue('settings.options', json);
              } catch {}
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
