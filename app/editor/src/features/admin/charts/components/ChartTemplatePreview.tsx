import { AxiosError } from 'axios';
import React from 'react';
import { FaEraser, FaPlay } from 'react-icons/fa6';
import { useChartTemplates } from 'store/hooks/admin';
import { Col, IChartPreviewRequestModel, mergeChartSettings, Row } from 'tno-core';

import { useChartTemplateContext } from '../ChartTemplateContext';
import * as styled from './styled';

/**
 * The page used to view and edit a chart template.
 * @returns Component.
 */
export const ChartTemplatePreview = () => {
  const { chartRequestForm, setChartRequestForm, filter, chartData } = useChartTemplateContext();
  const [, { previewBase64 }] = useChartTemplates();

  const handleGenerateBase64 = React.useCallback(
    async (preview: IChartPreviewRequestModel) => {
      try {
        if (preview.settings.scaleCalcMax) {
          // Automatically apply max scale based on dataset values.
          const scaleSuggestedMax =
            Math.max(...(preview.chartData as any)?.datasets.map((ds: any) => Math.max(ds.data))) +
            preview.settings.scaleCalcMax;
          preview.settings = mergeChartSettings({}, preview.settings, { scaleSuggestedMax });
        }
        const response = await previewBase64(preview);
        setChartRequestForm((form) => ({ ...form, chartBase64: response }));
      } catch (ex) {
        if (ex instanceof SyntaxError) {
          const message = ex.message;
          setChartRequestForm((form) => ({ ...form, chartBase64: message }));
        } else if (ex instanceof AxiosError) {
          const response = ex.response;
          const data = response?.data as any;
          setChartRequestForm((form) => ({ ...form, chartBase64: JSON.stringify(data) }));
        }
      }
    },
    [previewBase64, setChartRequestForm],
  );

  return (
    <styled.ChartTemplatePreview>
      <Row gap="1rem">
        <h3>Chart</h3>
        <Col flex="1" alignContent="center">
          Execute the current configuration to preview the chart.
        </Col>
        <Row gap="0.5rem">
          <FaPlay
            className="icon-btn"
            title="Generate Chart"
            onClick={() =>
              handleGenerateBase64({
                ...chartRequestForm,
                filter: filter ? JSON.parse(filter) : JSON.parse('{}'),
                chartData: chartData ? JSON.parse(chartData) : undefined,
              })
            }
          />
          <FaEraser
            className="icon-btn"
            title="Clear Chart"
            onClick={() => {
              setChartRequestForm({ ...chartRequestForm, chartBase64: '' });
            }}
          />
        </Row>
      </Row>
      <Col className="preview-image">
        <img alt="graph" src={chartRequestForm.chartBase64 ?? ''} />
      </Col>
    </styled.ChartTemplatePreview>
  );
};
