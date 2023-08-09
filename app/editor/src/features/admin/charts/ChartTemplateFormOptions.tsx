import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { Button, ButtonVariant, Checkbox, Col, Row } from 'tno-core';

import { useChartTemplateContext } from './ChartTemplateContext';
import {
  defaultChartJSOptions,
  defaultChartJSStackedOptions,
  defaultChartTemplate,
} from './constants';

export interface IChartTemplateFormOptionsProps {}

/**
 * The page used to view and edit a chart template Chart.js options.
 * @returns Component.
 */
export const ChartTemplateFormOptions: React.FC<IChartTemplateFormOptionsProps> = () => {
  const { values, setFieldValue } = useChartTemplateContext();

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
              Chart JS Options
            </label>
            <p>Editing these options will change all reports that use this template.</p>
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
            label="Horizontal Graph"
            defaultChecked={values.settings.options.indexAxis === 'y'}
            onChange={(e) => {
              const options = {
                ...values.settings.options,
                indexAxis: e.currentTarget.checked ? 'y' : 'x',
              };
              setChartOptions(JSON.stringify(options, null, 2));
              setFieldValue('settings.options', options);
            }}
          />
          <Checkbox
            name="showDataValues"
            label="Show Data Values"
            defaultChecked={values.settings.options.plugins?.datalabels?.labels?.title?.display}
            onChange={(e) => {
              const options = {
                ...values.settings.options,
                plugins: {
                  ...values.settings.options.plugins,
                  datalabels: {
                    ...values.settings.options.plugins.datalabels,
                    labels: {
                      ...values.settings.options.plugins.datalabels.labels,
                      title: {
                        ...values.settings.options.plugins.datalabels.labels.title,
                        display: e.currentTarget.checked,
                      },
                    },
                  },
                },
              };
              setChartOptions(JSON.stringify(options, null, 2));
              setFieldValue('settings.options', options);
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
