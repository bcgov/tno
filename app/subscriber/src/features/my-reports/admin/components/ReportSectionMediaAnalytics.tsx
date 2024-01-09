import { Action } from 'components/action';
import { Button } from 'components/button';
import { chartTypeOptions, groupByOptions } from 'features/my-reports/constants';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight } from 'react-icons/fa';
import { FaTrash } from 'react-icons/fa6';
import { useFilters, useFolders } from 'store/hooks';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getSortableOptions,
  IChartTemplateModel,
  IOptionItem,
  IReportSectionChartTemplateModel,
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

export interface IReportSectionMediaAnalyticsProps {
  index: number;
}

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionMediaAnalytics = React.forwardRef<
  HTMLDivElement,
  IReportSectionMediaAnalyticsProps
>(({ index, ...rest }, ref) => {
  const { values, setFieldValue } = useFormikContext<IReportForm>();
  const [{ myFolders: folders }, { findMyFolders }] = useFolders();
  const [{ myFilters: filters }, { findMyFilters }] = useFilters();

  const [folderOptions, setFolderOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(folders),
  );
  const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(filters),
  );
  const [chartOptions] = React.useState(getSortableOptions(values.template?.chartTemplates ?? []));
  const [chart, setChart] = React.useState<IChartTemplateModel>();

  const section = values.sections[index];

  React.useEffect(() => {
    // TODO: Move to parent component so that it doesn't run multiple times.
    if (!folders.length) {
      findMyFolders()
        .then((folders) => {
          setFolderOptions(getSortableOptions(folders));
        })
        .catch(() => {});
    }
    if (!filters.length) {
      findMyFilters()
        .then((filters) => {
          setFilterOptions(getSortableOptions(filters));
        })
        .catch(() => {});
    }
    // Only do on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <Col gap="0.5rem">
      <Row gap="1rem">
        <FormikCheckbox
          name={`sections.${index}.settings.hideEmpty`}
          label="Hide When Section Is Empty"
        />
      </Row>
      <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
      <FormikTextArea name={`sections.${index}.description`} label="Summary text:" />
      <Row>
        <FormikCheckbox
          name={`sections.${index}.settings.useAllContent`}
          label="Generate charts with content in this report"
          tooltip="When checked this will generate the charts with all the content in the report"
        />
        <Show visible={!section.settings.useAllContent}>
          <FormikCheckbox
            name={`sections.${index}.settings.removeDuplicates`}
            label="Remove Duplicate Content"
            tooltip="Remove content from this section that is in above sections"
          />
        </Show>
      </Row>
      <Show visible={!section.settings.useAllContent}>
        <Col className="frm-in">
          <label>Data source</label>
          <p>
            Choose the data sources to populate this section of your report. You can select from
            your saved searches and/or your folders.
          </p>
          <Row gap="1rem">
            <Col flex="1" className="description">
              <FormikSelect
                name={`sections.${index}.filterId`}
                label="My saved searches"
                options={filterOptions}
                value={filterOptions.find((o) => o.value === section.filterId) ?? ''}
                onChange={(newValue: any) => {
                  const option = newValue as OptionItem;
                  const filter = filters.find((f) => f.id === option?.value);
                  if (filter) setFieldValue(`sections.${index}.filter`, filter);
                }}
              >
                <Button
                  disabled={!values.sections[index].filterId}
                  onClick={() =>
                    window.open(`/search/advanced/${values.sections[index].filterId}`, '_blank')
                  }
                >
                  <FaArrowAltCircleRight />
                </Button>
              </FormikSelect>
              {section.filter?.description && <p>{section.filter?.description}</p>}
            </Col>
            <Col flex="1" className="description">
              <FormikSelect
                name={`sections.${index}.folderId`}
                label="My folders"
                options={folderOptions}
                value={folderOptions.find((o) => o.value === section.folderId) ?? ''}
                onChange={(newValue: any) => {
                  const option = newValue as OptionItem;
                  const folder = folders.find((f) => f.id === option?.value);
                  if (folder) setFieldValue(`sections.${index}.folder`, folder);
                }}
              >
                <Button
                  disabled={!values.sections[index].folderId}
                  onClick={() =>
                    window.open(`/folders/${values.sections[index].folderId}`, '_blank')
                  }
                >
                  <FaArrowAltCircleRight />
                </Button>
              </FormikSelect>
              {section.folder?.description && <p>{section.folder?.description}</p>}
            </Col>
          </Row>
        </Col>
      </Show>
      <Show visible={!values.template?.chartTemplates.length}>
        <p className="error">
          This template does not currently support charts. Update the template and add the charts it
          supports.
        </p>
      </Show>
      <Show visible={!!values.template?.chartTemplates.length}>
        <p>Select and order the charts that will be displayed in this section.</p>
        <Row className="add-chart">
          <Select
            name="charts"
            options={chartOptions}
            value={chartOptions.find((c) => c.value === chart?.id) ?? ''}
            width="50ch"
            onChange={(e) => {
              const o = e as OptionItem;
              const chart = values.template?.chartTemplates.find((ct) => ct.id === o?.value);
              setChart(chart);
            }}
          />
          <Button
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
                    isHorizontal: false,
                    showDataValues: false,
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
            <Col key={ct.id}>
              <Row>
                <Col flex="1">
                  <b>{ct.name}</b>
                </Col>
                <Col flex="2">{ct.description}</Col>
                <Col>
                  <Action
                    icon={<FaTrash />}
                    onClick={() => {
                      let items = [...section.chartTemplates];
                      items.splice(ctIndex, 1);
                      setFieldValue(`sections.${index}.chartTemplates`, items);
                    }}
                  />
                </Col>
              </Row>
              <Row className="chart-settings">
                <FormikSelect
                  label="Chart Type"
                  name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.chartType`}
                  value={
                    chartTypeOptions.find((o) => o.value === ct.sectionSettings.chartType) ?? ''
                  }
                  options={chartTypeOptions.filter((o) => ct.settings.chartTypes.includes(o.value))}
                  isClearable={false}
                />
                <FormikSelect
                  label="Group By"
                  name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.groupBy`}
                  value={groupByOptions.find((o) => o.value === ct.sectionSettings.groupBy) ?? ''}
                  options={groupByOptions.filter((o) => ct.settings.groupBy.includes(o.value))}
                  isClearable={false}
                />
                <FormikText
                  label="Width"
                  name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.width`}
                  value={ct.sectionSettings.width ?? 500}
                  type="number"
                  width="10ch"
                />
                <FormikText
                  label="Height"
                  name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.height`}
                  value={ct.sectionSettings.height ?? 500}
                  type="number"
                  width="10ch"
                />
              </Row>
            </Col>
          ))}
        </Col>
      </Show>
    </Col>
  );
});
