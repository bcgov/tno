import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { AxiosError } from 'axios';
import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useChartTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  IChartPreviewRequestModel,
  IChartTemplateModel,
  Row,
  Text,
} from 'tno-core';

export interface IChartTemplateFormPreviewProps {
  filter: string;
  setFilter: React.Dispatch<React.SetStateAction<string>>;
}

/**
 * The page used to view and edit a chart template.
 * @returns Component.
 */
export const ChartTemplateFormPreview: React.FC<IChartTemplateFormPreviewProps> = ({
  filter,
  setFilter,
}) => {
  const { values } = useFormikContext<IChartTemplateModel>();
  const [, { previewJson, previewBase64 }] = useChartTemplates();

  const [preview, setPreview] = React.useState<IChartPreviewRequestModel>({
    chartType: 'bar',
    template: values.template,
    content: [],
  });
  const [chartData, setChartData] = React.useState('');
  const [graph, setGraph] = React.useState('');

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
    [previewJson],
  );

  const handleGenerateBase64 = React.useCallback(
    async (preview: IChartPreviewRequestModel) => {
      try {
        const response = await previewBase64(preview);
        setGraph(response);
      } catch (ex) {
        if (ex instanceof SyntaxError) {
          setGraph(ex.message);
        } else if (ex instanceof AxiosError) {
          const response = ex.response;
          const data = response?.data as any;
          setGraph(JSON.stringify(data));
        }
      }
    },
    [previewBase64],
  );

  return (
    <>
      <h2>{values.name}</h2>
      <Col className="code frm-in">
        <p>
          If you would like to dynamically find content, create a filter and click 'Generate JSON'.
          Or you can manually enter the Chart JSON data and click 'Generate Graph'.
        </p>
        <label htmlFor="txa-template">Elasticsearch Filter</label>
        <Col className="editor">
          <Editor
            id="txa-filter"
            value={filter}
            onValueChange={(code) => setFilter(code)}
            highlight={(code) => {
              return highlight(code, languages.json, 'razor');
            }}
          />
        </Col>
      </Col>
      <Button
        variant={ButtonVariant.success}
        onClick={() =>
          handleGenerateJson({
            ...preview,
            filter: filter ? JSON.parse(filter) : JSON.parse('{}'),
          })
        }
      >
        Generate JSON
      </Button>
      <Col className="code frm-in">
        <label htmlFor="txa-json">Chart JSON</label>
        <Col className="editor">
          <Editor
            id="txa-json"
            value={chartData}
            onValueChange={(code) => setChartData(code)}
            highlight={(code) => {
              return highlight(code, languages.json, 'razor');
            }}
          />
        </Col>
      </Col>
      <hr />
      <Row>
        <Text
          name="chartType"
          label="Chart Type"
          value={preview.chartType}
          onChange={(e) => setPreview({ ...preview, chartType: e.target.value })}
        />
        <Text
          name="width"
          label="Width"
          value={preview.width}
          type="number"
          onChange={(e) => setPreview({ ...preview, width: parseInt(e.target.value) })}
        />
        <Text
          name="height"
          label="Height"
          value={preview.height}
          type="number"
          onChange={(e) => setPreview({ ...preview, height: parseInt(e.target.value) })}
        />
      </Row>
      <Button
        variant={ButtonVariant.success}
        onClick={() =>
          handleGenerateBase64({
            ...preview,
            filter: filter ? JSON.parse(filter) : JSON.parse('{}'),
            chartData: chartData ? JSON.parse(chartData) : undefined,
          })
        }
      >
        Generate Graph
      </Button>
      <Col className="preview-image">
        <img alt="graph" src={graph} />
      </Col>
    </>
  );
};
