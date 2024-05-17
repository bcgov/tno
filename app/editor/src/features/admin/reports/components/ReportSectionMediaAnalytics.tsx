import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight } from 'react-icons/fa';
import { useFilters, useFolders, useReports } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getSortableOptions,
  IChartTemplateModel,
  IReportModel,
  IReportSectionChartTemplateModel,
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

import { getSortableItems } from '../utils';
import { ReportSectionMediaAnalyticsChart } from './ReportSectionMediaAnalyticsChart';

export interface IReportSectionMediaAnalyticsProps {
  index: number;
}

export const ReportSectionMediaAnalytics = ({ index }: IReportSectionMediaAnalyticsProps) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ filters }] = useFilters();
  const [{ folders }] = useFolders();
  const [{ reports }] = useReports();

  const [filterOptions, setFilterOptions] = React.useState(getSortableItems(filters));
  const [folderOptions, setFolderOptions] = React.useState(getSortableItems(folders));
  const [reportOptions, setReportOptions] = React.useState(getSortableItems(reports));
  const [chartOptions] = React.useState(getSortableOptions(values.template?.chartTemplates ?? []));
  const [chart, setChart] = React.useState<IChartTemplateModel>();

  const section = values.sections[index];
  const folder = folders.find((f) => f.id === section.folderId);
  const filter = filters.find((f) => f.id === section.filterId);

  React.useEffect(() => {
    setFilterOptions(getSortableItems(filters));
  }, [filters]);

  React.useEffect(() => {
    setFolderOptions(getSortableItems(folders));
  }, [folders]);

  React.useEffect(() => {
    setReportOptions(getSortableItems(reports));
  }, [reports]);

  return (
    <Col gap="1rem" className="section">
      <FormikText
        name={`sections.${index}.settings.label`}
        label="Heading"
        tooltip="A heading label to display at the beginning of the section"
        maxLength={100}
      />
      <FormikTextArea
        name={`sections.${index}.description`}
        label="Summary"
        tooltip="The summary will be displayed at the beginning of the section"
        placeholder="Executive summary for this section or the whole report"
      />
      <Col>
        <label>Options</label>
        <Row>
          <Show visible={!section.settings.useAllContent}>
            <FormikCheckbox
              name={`sections.${index}.settings.removeDuplicates`}
              label="Remove duplicate content"
              tooltip="Remove content from this section that is in above sections"
            />
          </Show>
          <Show visible={!!section.folderId || !!section.linkedReportId}>
            <FormikCheckbox
              name={`sections.${index}.settings.overrideExcludeHistorical`}
              label={`Include all content from linked ${
                section.folderId ? 'folder' : 'report'
              } even if in prior report`}
              tooltip="This overrides the report option 'Exclude stories that have been sent out in
              previous report' for this section only."
            />
          </Show>
          <FormikCheckbox
            name={`sections.${index}.settings.useAllContent`}
            label="Generate charts with content in this report"
            tooltip="When checked this will generate the charts with all the content in the report"
          />
          <FormikCheckbox
            name={`sections.${index}.settings.direction`}
            label="Horizontally align the next media analytic chart"
            tooltip="This controls the placement of the next media analytics chart"
            checked={section.settings.direction === 'row'}
            onChange={(e) => {
              setFieldValue(
                `sections.${index}.settings.direction`,
                e.target.checked ? 'row' : 'column',
              );
            }}
          />
        </Row>
      </Col>
      <Show visible={!section.settings.useAllContent}>
        <Col>
          <hr />
          <label>Content</label>
          <p>
            To automatically populate this section with content select a filter and/or a folder.
          </p>
          <Row gap="1rem">
            <Col flex="1" className="description">
              <FormikSelect
                name={`sections.${index}.filterId`}
                label="Filter"
                tooltip="Select a filter to populate this chart"
                options={filterOptions}
                value={filterOptions.find((o) => o.value === section.filterId) ?? ''}
                onChange={(newValue) => {
                  const option = newValue as OptionItem;
                  const filter = filters.find((f) => f.id === option?.value);
                  if (filter) setFieldValue(`sections.${index}.filter`, filter);
                }}
              >
                <Button
                  variant={ButtonVariant.secondary}
                  disabled={!values.sections[index].filterId}
                  onClick={() =>
                    window.open(`/admin/filters/${values.sections[index].filterId}`, '_blank')
                  }
                >
                  <FaArrowAltCircleRight />
                </Button>
              </FormikSelect>
              {filter?.description && <p>{filter?.description}</p>}
            </Col>
            <Col flex="1" className="description">
              <FormikSelect
                name={`sections.${index}.folderId`}
                label="Folder"
                tooltip="Select a folder to populate this chart"
                options={folderOptions}
                value={folderOptions.find((o) => o.value === section.folderId) ?? ''}
                onChange={(newValue) => {
                  const option = newValue as OptionItem;
                  const folder = folders.find((f) => f.id === option?.value);
                  if (folder) setFieldValue(`sections.${index}.folder`, folder);
                }}
              >
                <Button
                  variant={ButtonVariant.secondary}
                  disabled={!values.sections[index].folderId}
                  onClick={() =>
                    window.open(`/admin/folders/${values.sections[index].folderId}`, '_blank')
                  }
                >
                  <FaArrowAltCircleRight />
                </Button>
              </FormikSelect>
              {folder?.description && <p>{folder?.description}</p>}
            </Col>
            <Col flex="1" className="description">
              <FormikSelect
                name={`sections.${index}.linkedReportId`}
                label="Report"
                tooltip="Select a report to populate this chart"
                options={reportOptions}
                value={reportOptions.find((o) => o.value === section.linkedReportId) ?? ''}
                onChange={(newValue) => {
                  const option = newValue as OptionItem;
                  const report = reports.find((f) => f.id === option?.value);
                  if (report) setFieldValue(`sections.${index}.linkedReport`, report);
                }}
              >
                <Button
                  variant={ButtonVariant.secondary}
                  disabled={!values.sections[index].folderId}
                  onClick={() =>
                    window.open(`/admin/folders/${values.sections[index].folderId}`, '_blank')
                  }
                >
                  <FaArrowAltCircleRight />
                </Button>
              </FormikSelect>
              {folder?.description && <p>{folder?.description}</p>}
            </Col>
          </Row>
        </Col>
      </Show>
      <Col>
        <hr />
        <label>Charts</label>
        <Show visible={!values.template?.chartTemplates.length}>
          <p className="error">
            This template does not currently support charts. Update the template and add the charts
            it supports.
          </p>
        </Show>
        <Show visible={!!values.template?.chartTemplates.length}>
          <p>Select and order the charts that will be displayed in this section.</p>
          <Row className="add-chart">
            <Select
              name="charts"
              options={chartOptions}
              value={chartOptions.find((c) => c.value === chart?.id) ?? ''}
              onChange={(e) => {
                const o = e as OptionItem;
                const chart = values.template?.chartTemplates.find((ct) => ct.id === o?.value);
                setChart(chart);
              }}
            />
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => {
                if (chart) {
                  const newChart: IReportSectionChartTemplateModel = {
                    ...chart,
                    sectionSettings: {
                      ...chart.sectionSettings,
                      title: '',
                      chartType: chart.settings?.chartTypes.length
                        ? chart.settings.chartTypes[0]
                        : '',
                      groupBy: chart.settings?.groupBy.length ? chart.settings.groupBy[0] : '',
                      dataset: chart.settings?.dataset.length ? chart.settings.dataset[0] : '',
                      datasetValue: chart.settings?.datasetValue.length
                        ? chart.settings.datasetValue[0]
                        : '',
                      isHorizontal: false,
                      showDataLabels: false,
                      width: 500,
                      height: 500,
                      options: { ...chart.settings?.options },
                    },
                  };
                  const charts = [...section.chartTemplates, newChart].map((ct, i) => {
                    return { ...ct, sortOrder: i };
                  });
                  setFieldValue(`sections.${index}.chartTemplates`, charts);
                  setChart(undefined);
                }
              }}
              disabled={!chart || section.chartTemplates.some((ct) => ct.id === chart.id)}
            >
              Add Chart
            </Button>
          </Row>
          <Col className="charts">
            {section.chartTemplates.map((ct, ctIndex) => (
              <ReportSectionMediaAnalyticsChart
                key={ct.id}
                sectionIndex={index}
                chartIndex={ctIndex}
              />
            ))}
          </Col>
        </Show>
      </Col>
    </Col>
  );
};
