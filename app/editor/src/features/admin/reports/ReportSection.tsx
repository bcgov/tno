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
  RadioGroup,
  ReportSectionTypeName,
  Row,
  Select,
  Show,
} from 'tno-core';

import { chartTypeOptions, groupByOptions } from '../charts/constants';
import { sectionOptions } from './constants';
import * as styled from './styled';

const getSortableItems = <T extends IFolderModel | IFilterModel>(items: T[]) => {
  return getSortableOptions(
    items,
    [],
    (f) =>
      new OptionItem(
        `${f.name}${f.owner?.username ? ` - [${f.owner.username}]` : ''}`,
        f.id,
        f.isEnabled,
      ),
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
  const [chartOptions] = React.useState(getSortableOptions(values.template?.chartTemplates ?? []));
  const [chart, setChart] = React.useState<IChartTemplateModel>();

  const section = values.sections[index];
  const folder = folders.find((f) => f.id === section.folderId);
  const filter = filters.find((f) => f.id === section.filterId);

  React.useEffect(() => {
    if (!filters.length)
      findAllFilters()
        .then((results) => setFilterOptions(getSortableItems(results)))
        .catch(() => {});
    if (!folders.length)
      findAllFolders()
        .then((results) => setFolderOptions(getSortableItems(results)))
        .catch(() => {});
    // Only fetch items on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.ReportSection className={className}>
      <Col gap="1rem">
        <Row gap="1rem">
          <Col>
            <FormikText
              name={`sections.${index}.settings.label`}
              label="Heading"
              tooltip="A heading label to display at the beginning of the section"
              maxLength={100}
            />
          </Col>
          <Col flex="1">
            <FormikTextArea
              name={`sections.${index}.description`}
              label="Summary"
              tooltip="The summary will be displayed at the beginning of the section"
              placeholder="Executive summary for this section or the whole report"
            />
          </Col>
        </Row>
        <Row gap="1rem">
          <Row className="section-type" flex="1" gap="1rem" justifyItems="stretch">
            <RadioGroup
              name="settings"
              label="Section Type"
              options={sectionOptions}
              value={sectionOptions.find((o) => o.value === section.settings.sectionType) ?? ''}
              onChange={(e) => {
                const value = e.target.value;
                setFieldValue(`sections.${index}`, {
                  ...section,
                  folderId: undefined,
                  filterId: undefined,
                  settings: {
                    ...section.settings,
                    sectionType: value,
                    showHeadlines: [
                      ReportSectionTypeName.Content,
                      ReportSectionTypeName.TableOfContents,
                    ].some((o) => o === value),
                    showCharts:
                      value === ReportSectionTypeName.Summary ? true : section.settings.showCharts,
                  },
                });
              }}
            />
            <Col flex="1">
              <Show visible={section.settings.sectionType === ReportSectionTypeName.Content}>
                <p>
                  This section will display content from a filter and/or a folder. It can also
                  display charts in this section.
                </p>
              </Show>
              <Show
                visible={section.settings.sectionType === ReportSectionTypeName.TableOfContents}
              >
                <p>
                  This section will display a table of contents for all the content in the report.
                </p>
              </Show>
              <Show visible={section.settings.sectionType === ReportSectionTypeName.Summary}>
                <p>This section will only display section information and charts.</p>
              </Show>
            </Col>
          </Row>
          <Col className="frm-in" flex="1">
            <label>Options</label>
            <Col>
              <FormikCheckbox name={`sections.${index}.isEnabled`} label="Is Enabled" />
              <FormikCheckbox
                name={`sections.${index}.settings.hideEmpty`}
                label="Hide When Section Is Empty"
                tooltip="Hide this section if there is no content"
              />
              <Show
                visible={[
                  ReportSectionTypeName.Content,
                  ReportSectionTypeName.TableOfContents,
                ].includes(section.settings.sectionType)}
              >
                <FormikCheckbox
                  name={`sections.${index}.settings.showHeadlines`}
                  label="Show Headlines"
                  tooltip="Display the story headlines in this section.  This is similar to a table of contents, but only for this section"
                />
              </Show>
              <Show
                visible={[ReportSectionTypeName.Content].includes(section.settings.sectionType)}
              >
                <FormikCheckbox
                  name={`sections.${index}.settings.showFullStory`}
                  label="Show Full Story"
                  tooltip="Display the full story for each content item in this section"
                />
                <FormikCheckbox
                  name={`sections.${index}.settings.showImage`}
                  label="Show Image"
                  tooltip="Display the image for each content item in this section (if there is an image)"
                />
              </Show>
              <Show
                visible={[ReportSectionTypeName.Content].includes(section.settings.sectionType)}
              >
                <FormikCheckbox
                  name={`sections.${index}.settings.removeDuplicates`}
                  label="Remove Duplicate Content"
                  tooltip="Remove content from this section that is in above sections"
                />
              </Show>
              <Show
                visible={[ReportSectionTypeName.Content, ReportSectionTypeName.Summary].includes(
                  section.settings.sectionType,
                )}
              >
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
              </Show>
            </Col>
          </Col>
        </Row>
        <Show visible={[ReportSectionTypeName.Content].includes(section.settings.sectionType)}>
          <Col>
            <hr />
            <label>Content</label>
            <p>
              To automatically populate this section with content select a filter and/or a folder.
            </p>
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
                    const filter = filters.find((f) => f.id === option?.value);
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
                    const folder = folders.find((f) => f.id === option?.value);
                    if (folder) setFieldValue(`sections.${index}.folder`, folder);
                  }}
                />
                {folder?.description && <p>{folder?.description}</p>}
              </Col>
            </Row>
            <Row>
              <Col flex="1" className="description">
                <FormikSelect
                  label="Group Section Content By"
                  name={`settings.${index}.settings.groupBy`}
                  tooltip="Select a content field to group by within the section"
                  options={groupByOptions}
                  value={groupByOptions.find((o) => o.value === section.settings.groupBy) ?? ''}
                  onChange={(newValue) => {
                    const option = newValue as OptionItem;
                    const groupBy = groupByOptions.find((f) => f.value === option?.value);
                    setFieldValue(`sections.${index}.settings.groupBy`, groupBy?.value ?? '');
                  }}
                />
              </Col>
            </Row>
          </Col>
        </Show>
        <Show
          visible={[ReportSectionTypeName.Content, ReportSectionTypeName.Summary].includes(
            section.settings.sectionType,
          )}
        >
          <Show visible={section.settings.showCharts}>
            <Col>
              <hr />
              <label>Charts</label>
              <Show visible={!values.template?.chartTemplates.length}>
                <p className="error">
                  This template does not currently support charts. Update the template and add the
                  charts it supports.
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
                      const chart = values.template?.chartTemplates.find(
                        (ct) => ct.id === o?.value,
                      );
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
                            groupBy: chart.settings?.groupBy.length
                              ? chart.settings.groupBy[0]
                              : '',
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
                              chartTypeOptions.find(
                                (o) => o.value === ct.sectionSettings.chartType,
                              ) ?? ''
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
                              groupByOptions.find((o) => o.value === ct.sectionSettings.groupBy) ??
                              ''
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
        </Show>
      </Col>
    </styled.ReportSection>
  );
};
