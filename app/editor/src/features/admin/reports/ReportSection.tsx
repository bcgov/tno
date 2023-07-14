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
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

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
          name={`sections.${index}.name`}
          label="Name"
          tooltip="Unique name to identify the section"
          placeholder="Enter unique name"
          required
          maxLength={100}
        />
        <FormikText
          name={`sections.${index}.settings.label`}
          label="Section Title"
          tooltip="A title to display at the beginning of the section"
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
      <p>To automatically populate this section with content select a filter or folder.</p>
      <Row>
        <Col flex="1" className="description">
          <FormikSelect
            name={`sections.${index}.filterId`}
            label="Filter"
            tooltip="Select a filter to dynamically fill this section with content"
            options={filterOptions}
            value={filterOptions.find((o) => o.value === section.filterId) ?? ''}
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
          />
          {folder?.description && <p>{folder?.description}</p>}
        </Col>
      </Row>
      <Col className="frm-in">
        <label>Options</label>
        <Row>
          <FormikCheckbox
            name={`sections.${index}.settings.isSummary`}
            label="Is Summary Section"
            tooltip="A summary section contains the content from all sections"
            onChange={(e) => {
              setFieldValue(`sections.${index}.settings`, {
                ...values.sections[index].settings,
                showContent: e.target.checked ? false : true,
                showCharts: e.target.checked ? true : values.sections[index].settings.showCharts,
                isSummary: e.target.checked,
                removeDuplicates: e.target.checked,
              });
            }}
          />
          <FormikCheckbox
            name={`sections.${index}.settings.showContent`}
            label="Show Content"
            tooltip="Display the content in this section"
          />
          {!values.sections[index].settings.isSummary && (
            <FormikCheckbox
              name={`sections.${index}.settings.removeDuplicates`}
              label="Remove Duplicates"
              tooltip="Remove content from this section that is in above sections"
            />
          )}
        </Row>
      </Col>
      <hr />
      <Col className="frm-in">
        <label>Charts</label>
        <Row>
          <FormikCheckbox
            name={`sections.${index}.settings.showCharts`}
            label="Show Charts"
            tooltip="Display charts in this section"
          />
          {values.sections[index].settings.showCharts && (
            <FormikCheckbox
              name={`sections.${index}.settings.chartsOnTop`}
              label="Show Charts on Top of Content"
              tooltip="Place the charts before the content"
            />
          )}
        </Row>
      </Col>
      {values.sections[index].settings.showCharts && (
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
                  const charts = [...values.sections[index].chartTemplates, { ...chart }].map(
                    (ct, i) => {
                      return { ...ct, sortOrder: i };
                    },
                  );
                  setFieldValue(`sections.${index}.chartTemplates`, charts);
                  setChart(undefined);
                }}
                disabled={
                  !chart || values.sections[index].chartTemplates.some((ct) => ct.id === chart.id)
                }
              >
                Add Chart
              </Button>
            </Row>
            <Col className="charts">
              {values.sections[index].chartTemplates.map((ct, ctIndex) => (
                <Row key={ct.id}>
                  <Col flex="1">{ct.name}</Col>
                  <Col flex="2">{ct.description}</Col>
                  <Col>
                    <Button
                      variant={ButtonVariant.danger}
                      onClick={() => {
                        let items = [...values.sections[index].chartTemplates];
                        items.splice(ctIndex, 1);
                        setFieldValue(`sections.${index}.chartTemplates`, items);
                      }}
                    >
                      <FaTrash />
                    </Button>
                  </Col>
                </Row>
              ))}
            </Col>
          </Show>
        </Col>
      )}
    </styled.ReportSection>
  );
};
