import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { AxiosError } from 'axios';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import { useChartTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  IChartPreviewRequestModel,
  OptionItem,
  Row,
  Select,
  Text,
} from 'tno-core';

import { useChartTemplateContext } from './ChartTemplateContext';
import { chartTypeOptions, defaultChartTemplate, groupByOptions } from './constants';

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
  const { values, setFieldValue, preview, setPreview } = useChartTemplateContext();
  const [, { previewJson, previewBase64 }] = useChartTemplates();

  const [chartData, setChartData] = React.useState(
    preview.chartData ? JSON.stringify(preview.chartData, null, 2) : undefined,
  );
  const [chartTypes] = React.useState(
    values.settings?.chartTypes?.map((ct) => chartTypeOptions.find((o) => o.value === ct)) ?? [],
  );
  const [groupBys] = React.useState(
    values.settings?.groupBy?.map((ct) => groupByOptions.find((o) => o.value === ct)) ?? [],
  );
  const [indexOptions] = React.useState([
    new OptionItem('Published', 'content'),
    new OptionItem('Unpublished', 'unpublished_content'),
  ]);

  React.useEffect(() => {
    // Initializes settings if the DB has no value.
    if (!values.settings) {
      setFieldValue('settings', defaultChartTemplate.settings);
    }
  }, [setFieldValue, values.settings]);

  React.useEffect(() => {
    if (chartData) {
      try {
        const data = JSON.parse(chartData);
        setPreview((preview) => ({ ...preview, chartData: data }));
      } catch {}
    }
  }, [chartData, setPreview]);

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
        setPreview((preview) => ({ ...preview, chartBase64: response }));
      } catch (ex) {
        if (ex instanceof SyntaxError) {
          const message = ex.message;
          setPreview((preview) => ({ ...preview, chartBase64: message }));
        } else if (ex instanceof AxiosError) {
          const response = ex.response;
          const data = response?.data as any;
          setPreview((preview) => ({ ...preview, chartBase64: JSON.stringify(data) }));
        }
      }
    },
    [previewBase64, setPreview],
  );

  return (
    <>
      <h2>{values.name}</h2>
      <Col className="code frm-in">
        <p>
          If you would like to dynamically find content, create a filter and click 'Generate JSON'.
          Or you can manually enter the Chart JSON data and click 'Generate Graph'.
        </p>

        <Row>
          <Select
            label="Chart Type"
            name="chartType"
            options={chartTypes}
            value={chartTypes.find((o) => o?.value === preview.settings.chartType) ?? ''}
            isClearable={false}
            width="200px"
            onChange={(newValue) => {
              const option = newValue as OptionItem;
              setPreview({
                ...preview,
                settings: { ...preview.settings, chartType: option.value?.toString() ?? 'bar' },
              });
            }}
          />
          <Select
            label="Group By"
            name="groupBy"
            options={groupBys}
            value={groupByOptions.find((o) => o?.value === preview.settings.groupBy) ?? ''}
            isClearable={false}
            width="200px"
            onChange={(newValue) => {
              const option = newValue as OptionItem;
              setPreview({
                ...preview,
                settings: { ...preview.settings, groupBy: option.value?.toString() ?? 'bar' },
              });
            }}
          />
          <Text
            name="width"
            label="Width"
            value={preview.settings.width ?? ''}
            type="number"
            onChange={(e) =>
              setPreview({
                ...preview,
                settings: { ...preview.settings, width: parseInt(e.target.value) },
              })
            }
          />
          <Text
            name="height"
            label="Height"
            value={preview.settings.height ?? ''}
            type="number"
            onChange={(e) =>
              setPreview({
                ...preview,
                settings: { ...preview.settings, height: parseInt(e.target.value) },
              })
            }
          />
        </Row>
        <hr />
        <Row>
          <Select
            label="Index"
            name="index"
            options={indexOptions}
            value={indexOptions.find((o) => o.value === preview.index) ?? ''}
            clearValue={''}
            onChange={(newValue) => {
              const option = newValue as OptionItem;
              setPreview((preview) => ({ ...preview, index: option.value?.toString() }));
            }}
          />
        </Row>
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
      <hr />
      <Row>
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
        <Button
          variant={ButtonVariant.secondary}
          onClick={() => {
            setChartData('');
            setPreview({ ...preview, chartData: undefined });
          }}
        >
          Clear
        </Button>
      </Row>
      <Col className="code frm-in">
        <label htmlFor="txa-json">Chart JSON</label>
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
      <hr />
      <Row>
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
          Generate Chart
        </Button>
        <Button
          variant={ButtonVariant.secondary}
          onClick={() => {
            setPreview({ ...preview, chartBase64: '' });
          }}
        >
          Clear
        </Button>
      </Row>
      <Col className="preview-image">
        <img alt="graph" src={preview.chartBase64 ?? ''} />
      </Col>
    </>
  );
};
