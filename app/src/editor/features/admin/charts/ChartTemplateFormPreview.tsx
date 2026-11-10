import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import React from 'react';
import { Col } from 'tno-core';

import { useChartTemplateContext } from './ChartTemplateContext';
import {
  ChartTemplatePreview,
  ChartTemplatePreviewConfig,
  ChartTemplatePreviewJson,
  ChartTemplatePreviewOptions,
  ChartTemplatePreviewSources,
} from './components';
import { defaultChartTemplate } from './constants';

/**
 * The page used to view and edit a chart template.
 * @returns Component.
 */
export const ChartTemplateFormPreview = () => {
  const { values, setFieldValue } = useChartTemplateContext();

  React.useEffect(() => {
    // Initializes settings if the DB has no value.
    if (!values.settings) {
      setFieldValue('settings', defaultChartTemplate.settings);
    }
  }, [setFieldValue, values.settings]);

  return (
    <Col className="frm-in" gap="1rem">
      <h2>{values.name}</h2>
      <p>
        If you would like to dynamically find content, create a filter and click 'Generate JSON'. Or
        you can manually enter the Chart JSON data and click 'Generate Graph'.
      </p>
      <ChartTemplatePreviewSources />
      <ChartTemplatePreviewOptions />
      <ChartTemplatePreviewConfig />
      <ChartTemplatePreviewJson />
      <ChartTemplatePreview />
      <hr />
    </Col>
  );
};
