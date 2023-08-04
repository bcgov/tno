import { useFormikContext } from 'formik';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import { useChartTemplates } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  getSortableOptions,
  IChartTemplateModel,
  IReportTemplateModel,
  OptionItem,
  Row,
  Select,
} from 'tno-core';

/**
 * The page used to view and edit a report template.
 * @returns Component.
 */
export const ReportTemplateFormCharts: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IReportTemplateModel>();
  const [{ chartTemplates }, { findAllChartTemplates }] = useChartTemplates();

  const [chartOptions, setChartOptions] = React.useState(getSortableOptions(chartTemplates));
  const [chart, setChart] = React.useState<IChartTemplateModel>();

  React.useEffect(() => {
    if (!chartTemplates.length)
      findAllChartTemplates()
        .then((results) => setChartOptions(getSortableOptions(results)))
        .catch();
    // Only fetch items on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <>
      <Col>
        <h2>{values.name}</h2>
        <p>Select the charts that are supported by this template.</p>
      </Col>
      {/* <Row>
        <Col flex="2" className="frm-in">
          <label>Template Options</label>
          <p>
            Enable the following options to provide sections, summaries, and charts. These are used
            to control which options are available to users when they are configuring their report.
            Only enable the options that are implemented in the template.
          </p>
          <Row className="enable-options">
            <FormikCheckbox
              label="Enable Section Charts"
              name="settings.enableSectionCharts"
              tooltip="This template support charts within each section"
            />
            <FormikCheckbox
              label="Enable Summary Charts"
              name="settings.enableSummaryCharts"
              tooltip="This template supports a summary section"
            />
          </Row>
        </Col>
      </Row> */}
      <Col>
        <Row className="add-chart">
          <Select
            name="charts"
            label="Charts"
            options={chartOptions}
            value={chartOptions.find((c) => c.value === chart?.id) ?? ''}
            onChange={(e) => {
              const o = e as OptionItem;
              const chart = chartTemplates.find((ct) => ct.id === o?.value);
              setChart(chart);
            }}
          />
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              const charts = [...values.chartTemplates, { ...chart }].map((ct, i) => {
                return { ...ct, sortOrder: i };
              });
              setFieldValue(`chartTemplates`, charts);
              setChart(undefined);
            }}
            disabled={!chart || values.chartTemplates.some((ct) => ct.id === chart.id)}
          >
            Add Chart
          </Button>
        </Row>
        <Col className="charts">
          {values.chartTemplates.map((ct, ctIndex) => (
            <Row key={ct.id}>
              <Col flex="1">{ct.name}</Col>
              <Col flex="2">{ct.description}</Col>
              <Col>
                <Button
                  variant={ButtonVariant.danger}
                  onClick={() => {
                    let items = [...values.chartTemplates];
                    items.splice(ctIndex, 1);
                    setFieldValue(`chartTemplates`, items);
                  }}
                >
                  <FaTrash />
                </Button>
              </Col>
            </Row>
          ))}
        </Col>
      </Col>
    </>
  );
};
