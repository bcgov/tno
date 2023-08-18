import { useFormikContext } from 'formik';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import { useFilters, useFolders } from 'store/hooks/admin';
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
  IFilterModel,
  IFolderModel,
  IReportModel,
  IReportSectionChartTemplateModel,
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

import { chartTypeOptions, groupByOptions } from '../charts/constants';
import * as styled from './styled';

const getSortableItems = <T extends IFolderModel | IFilterModel>(items: T[]) => {
  return getSortableOptions(
    items,
    [],
    (f) => new OptionItem(`${f.name} - [${f.owner?.username}]`, f.id, f.isEnabled),
    (a, b) => {
      if (a.owner !== undefined && b.owner !== undefined) {
        if (a.owner.username < b.owner.username) return -1;
        if (a.owner.username > b.owner.username) return 1;
      }
      if (a.sortOrder < b.sortOrder) return -1;
      if (a.sortOrder > b.sortOrder) return 1;
      if (a.name < b.name) return -1;
      if (a.name > b.name) return 1;
      return 0;
    },
  );
};

export interface IReportSectionProps {
  /** Class name of component. */
  className?: string;
  /** Index position within sections. */
  index: number;
}

/**
 * Provides a component to configure the report section.
 * @param param0 Component properties.
 * @returns A component.
 */
export const ReportSection: React.FC<IReportSectionProps> = ({ className, index }) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ filters }, { findAllFilters }] = useFilters();
  const [{ folders }, { findAllFolders }] = useFolders();

  const [filterOptions, setFilterOptions] = React.useState(getSortableItems(filters));
  const [folderOptions, setFolderOptions] = React.useState(getSortableItems(folders));
  const [chartOptions] = React.useState(getSortableOptions(values.template.chartTemplates));
  const [chart, setChart] = React.useState<IChartTemplateModel>();

  const section = values.sections[index];
  const folder = folders.find((f) => f.id === section.folderId);
  const filter = filters.find((f) => f.id === section.filterId);

  React.useEffect(() => {
    if (!filters.length)
      findAllFilters()
        .then((results) => setFilterOptions(getSortableItems(results)))
        .catch();
    if (!folders.length)
      findAllFolders()
        .then((results) => setFolderOptions(getSortableItems(results)))
        .catch();
    // Only fetch items on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.ReportSection className={className}>
      <Row alignItems="center">
        <FormikText
          name={`sections.${index}.settings.label`}
          label="Heading"
          tooltip="A heading label to display at the beginning of the section"
          maxLength={100}
        />
        <FormikCheckbox name={`sections.${index}.isEnabled`} label="Is Enabled" />
      </Row>
      <FormikTextArea
        name={`sections.${index}.description`}
        label="Summary"
        tooltip="The summary will be displayed at the beginning of the section"
        placeholder="Describe the purpose of this section"
      />
      <Col className="frm-in">
        <label>Options</label>
        <Row>
          <FormikCheckbox
            name={`sections.${index}.settings.isSummary`}
            label="Is Summary Section"
            tooltip="A summary section contains the content from all the sections above this section"
            onChange={(e) => {
              setFieldValue(`sections.${index}`, {
                ...section,
                folderId: undefined,
                filterId: undefined,
                settings: {
                  ...section.settings,
                  showContent: e.target.checked ? false : true,
                  showCharts: e.target.checked ? true : section.settings.showCharts,
                  isSummary: e.target.checked,
                  removeDuplicates: false,
                },
              });
            }}
          />
          <Show visible={!section.settings.isSummary}>
            <FormikCheckbox
              name={`sections.${index}.settings.showContent`}
              label="Show Content"
              tooltip="Display the content in this section"
            />
            <FormikCheckbox
              name={`sections.${index}.settings.removeDuplicates`}
              label="Remove Duplicates"
              tooltip="Remove content from this section that is in above sections"
            />
          </Show>
        </Row>
      </Col>
      <Show visible={!section.settings.isSummary}>
        <p>To automatically populate this section with content select a filter or folder.</p>
        <Row>
          <Col flex="1" className="description">
            <FormikSelect
              name={`sections.${index}.filterId`}
              label="Filter"
              tooltip="Select a filter to dynamically fill this section with content"
              options={filterOptions}
              value={filterOptions.find((o) => o.value === section.filterId) ?? ''}
              onChange={(newValue) => {
                const option = newValue as OptionItem;
                const filter = filters.find((f) => f.id === option.value);
                if (filter) setFieldValue(`sections.${index}.filter`, filter);
              }}
            />
            {filter?.description && <p>{filter?.description}</p>}
          </Col>
          <Col flex="1" className="description">
            <FormikSelect
              name={`sections.${index}.folderId`}
              label="Folder"
              tooltip="Select a folder to fill this section with its content"
              options={folderOptions}
              value={folderOptions.find((o) => o.value === section.folderId) ?? ''}
              onChange={(newValue) => {
                const option = newValue as OptionItem;
                const folder = folders.find((f) => f.id === option.value);
                if (folder) setFieldValue(`sections.${index}.folder`, folder);
              }}
            />
            {folder?.description && <p>{folder?.description}</p>}
          </Col>
        </Row>
      </Show>
      <hr />
      <Col className="frm-in">
        <label>Charts</label>
        <Row>
          <FormikCheckbox
            name={`sections.${index}.settings.showCharts`}
            label="Show Charts"
            tooltip="Display charts in this section"
          />
          {section.settings.showCharts && (
            <FormikCheckbox
              name={`sections.${index}.settings.chartsOnTop`}
              label="Show Charts on Top of Content"
              tooltip="Place the charts before the content"
            />
          )}
        </Row>
      </Col>
      <Show visible={section.settings.showCharts}>
        <Col>
          <Show visible={!values.template.chartTemplates.length}>
            <p className="error">
              This template does not currently support charts. Update the template and add the
              charts it supports.
            </p>
          </Show>
          <Show visible={!!values.template.chartTemplates.length}>
            <p>Select and order the charts that will be displayed in this section.</p>
            <Row className="add-chart">
              <Select
                name="charts"
                options={chartOptions}
                value={chartOptions.find((c) => c.value === chart?.id) ?? ''}
                onChange={(e) => {
                  const o = e as OptionItem;
                  const chart = values.template.chartTemplates.find((ct) => ct.id === o?.value);
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
                        chartType: chart.settings.chartTypes.length
                          ? chart.settings.chartTypes[0]
                          : '',
                        groupBy: chart.settings.groupBy.length ? chart.settings.groupBy[0] : '',
                        isHorizontal: false,
                        showDataValues: false,
                        width: 500,
                        height: 500,
                        options: { ...chart.settings.options },
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
                <Row key={ct.id}>
                  <Col flex="1">
                    <Row>
                      <Col flex="1">
                        <b>{ct.name}</b>
                      </Col>
                      <Col flex="2">{ct.description}</Col>
                      <Col>
                        <Button
                          variant={ButtonVariant.danger}
                          onClick={() => {
                            let items = [...section.chartTemplates];
                            items.splice(ctIndex, 1);
                            setFieldValue(`sections.${index}.chartTemplates`, items);
                          }}
                        >
                          <FaTrash />
                        </Button>
                      </Col>
                    </Row>
                    <Row className="chart-settings">
                      {/* <FormikText
                        label="Title"
                        name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.title`}
                        value={ct.sectionSettings.title}
                        UPDATE VALUE IN OPTIONS
                      /> */}
                      <FormikSelect
                        label="Chart Type"
                        name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.chartType`}
                        value={
                          chartTypeOptions.find((o) => o.value === ct.sectionSettings.chartType) ??
                          ''
                        }
                        options={chartTypeOptions.filter((o) =>
                          ct.settings.chartTypes.includes(o.value),
                        )}
                        isClearable={false}
                      />
                      <FormikSelect
                        label="Group By"
                        name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.groupBy`}
                        value={
                          groupByOptions.find((o) => o.value === ct.sectionSettings.groupBy) ?? ''
                        }
                        options={groupByOptions.filter((o) =>
                          ct.settings.groupBy.includes(o.value),
                        )}
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
                </Row>
              ))}
            </Col>
          </Show>
        </Col>
      </Show>
    </styled.ReportSection>
  );
};
