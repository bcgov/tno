import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { AxiosError } from 'axios';
import { highlight, languages } from 'prismjs';
import React from 'react';
import { FaAngleDown, FaEraser, FaMinus, FaPlay } from 'react-icons/fa6';
import Editor from 'react-simple-code-editor';
import { useChartTemplates } from 'store/hooks/admin';
import { Col, IChartPreviewRequestModel, Row, ToggleButton } from 'tno-core';

import { useChartTemplateContext } from '../ChartTemplateContext';
import { defaultChartTemplate } from '../constants';
import * as styled from './styled';

/**
 * The page used to view and edit a chart template.
 * @returns Component.
 */
export const ChartTemplatePreviewJson = () => {
  const {
    values,
    setFieldValue,
    chartRequestForm,
    setChartRequestForm,
    filter,
    chartData,
    setChartData,
  } = useChartTemplateContext();
  const [, { previewJson }] = useChartTemplates();

  const [show, setShow] = React.useState(false);

  React.useEffect(() => {
    // Initializes settings if the DB has no value.
    if (!values.settings) {
      setFieldValue('settings', defaultChartTemplate.settings);
    }
  }, [setFieldValue, values.settings]);

  const handleGenerateJson = React.useCallback(
    async (preview: IChartPreviewRequestModel) => {
      try {
        const response = await previewJson(preview);
        setChartData(JSON.stringify(response.json, null, 2));
      } catch (ex) {
        if (ex instanceof SyntaxError) {
          setChartData(ex.message);
        } else if (ex instanceof AxiosError) {
          const response = ex.response;
          const data = response?.data as any;
          setChartData(JSON.stringify(data));
        }
      }
    },
    [previewJson, setChartData],
  );

  return (
    <styled.ChartTemplatePreviewJson>
      <Row justifyContent="space-between">
        <h3>Chart JSON</h3>
        <span>Execute the configured data source, or manually enter chart JSON data.</span>
        <Row gap="0.5rem">
          <FaPlay
            className="icon-btn"
            title="Generate JSON"
            onClick={() =>
              handleGenerateJson({
                ...chartRequestForm,
                filter: filter ? JSON.parse(filter) : JSON.parse('{}'),
              })
            }
          />
          <FaEraser
            className="icon-btn"
            title="Clear JSON"
            onClick={() => {
              setChartData('');
              setChartRequestForm({ ...chartRequestForm, chartData: undefined });
            }}
          />
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
              id="txa-json"
              value={chartData ?? ''}
              onValueChange={(code) => setChartData(code)}
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
