import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { highlight, languages } from 'prismjs';
import React from 'react';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';
import Editor from 'react-simple-code-editor';
import { Col, mergeChartSettings, Row, ToggleButton } from 'tno-core';

import { useChartTemplateContext } from '../ChartTemplateContext';
import * as styled from './styled';

/**
 * The page used to view and edit a chart template.
 * @returns Component.
 */
export const ChartTemplatePreviewConfig = () => {
  const { values, chartRequestForm, setChartRequestForm } = useChartTemplateContext();

  const [show, setShow] = React.useState(false);
  const [chartOptions, setChartOptions] = React.useState(
    JSON.stringify(chartRequestForm.settings?.options ?? '{}', null, 2),
  );

  React.useEffect(() => {
    if (chartRequestForm.settings?.options)
      setChartOptions(JSON.stringify(chartRequestForm.settings?.options ?? '{}', null, 2));
  }, [chartRequestForm.settings?.options]);

  React.useEffect(() => {
    if (values.settings?.options) {
      setChartRequestForm({
        ...chartRequestForm,
        settings: mergeChartSettings(values.settings.options, chartRequestForm.settings, {}),
      });
    }
    // Only fire when the options are changed from the Config tab
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values.settings.options]);

  return (
    <styled.ChartTemplatePreviewJson>
      <Row justifyContent="space-between">
        <h3>Chart Configuration</h3>
        <span>Review or manually enter chart configuration.</span>
        <Row gap="0.5rem">
          <ToggleButton
            on={<FaMinus />}
            off={<FaAngleDown />}
            onClick={() => setShow(!show)}
            value={show}
          />
        </Row>
      </Row>
      {show && (
        <Col className="code">
          <Col className="editor">
            <Editor
              id="txa-configuration"
              value={chartOptions}
              onValueChange={(code) => {
                try {
                  setChartOptions(code);
                  const json = JSON.parse(code);
                  setChartRequestForm((form) => ({
                    ...form,
                    settings: { ...form.settings, options: json },
                  }));
                } catch {}
              }}
              highlight={(code) => {
                return highlight(code, languages.json, 'razor');
              }}
            />
          </Col>
        </Col>
      )}
    </styled.ChartTemplatePreviewJson>
  );
};
