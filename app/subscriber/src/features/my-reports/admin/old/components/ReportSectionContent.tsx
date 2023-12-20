import { Button } from 'components/button';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight } from 'react-icons/fa';
import { useFilters, useFolders } from 'store/hooks';
import {
  Checkbox,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getReportSectionOrderByOptions,
  getSortableOptions,
  IOptionItem,
  OptionItem,
  ReportSectionTypeName,
  Row,
  Show,
} from 'tno-core';

import { IReportForm } from '../../../interfaces';
import { IReportSectionProps } from './ReportSection';
import { ReportSectionCharts } from './ReportSectionCharts';

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionContent = React.forwardRef<HTMLDivElement, IReportSectionProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useFormikContext<IReportForm>();
    const [{ myFolders: folders }, { findMyFolders }] = useFolders();
    const [{ myFilters: filters }, { findMyFilters }] = useFilters();

    const [folderOptions, setFolderOptions] = React.useState<IOptionItem[]>(
      getSortableOptions(folders),
    );
    const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>(
      getSortableOptions(filters),
    );
    const [orderOptions] = React.useState<IOptionItem[]>(getReportSectionOrderByOptions());

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
          <Show visible={section.settings.showCharts}>
            <Checkbox
              name="sectionType"
              label="Summary of stories above"
              value={section.settings.sectionType === ReportSectionTypeName.Summary}
              onChange={(e) => {
                setFieldValue(`sections.${index}`, {
                  ...section,
                  filterId: undefined,
                  filter: undefined,
                  folderId: undefined,
                  folder: undefined,
                  settings: {
                    ...section.settings,
                    sectionType: e.target.checked
                      ? ReportSectionTypeName.Summary
                      : ReportSectionTypeName.Content,
                    removeDuplicates: false,
                  },
                });
              }}
            />
          </Show>
          <FormikCheckbox
            name={`sections.${index}.settings.hideEmpty`}
            label="Hide When Section Is Empty"
          />
        </Row>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <FormikTextArea name={`sections.${index}.description`} label="Summary text:" />
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
                    window.open(
                      `/search?modify=${values.sections[index].filterId}&name=${
                        values.sections[index].filter?.name ?? ''
                      }`,
                      '_blank',
                    )
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
        <Row>
          <Col flex="1" className="description">
            <FormikCheckbox
              name={`sections.${index}.settings.removeDuplicates`}
              label="Remove Duplicate Content"
              tooltip="Remove content from this section that is in above sections"
            />
            <Show visible={!section.settings.showCharts}>
              <FormikCheckbox
                name={`sections.${index}.settings.showHeadlines`}
                label="Show Headlines"
                tooltip="Display the story headline for each content item in this section"
              />
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
          </Col>
          <Col flex="1" className="description">
            <FormikSelect
              name={`sections.${index}.settings.orderByField`}
              label="Order by"
              options={orderOptions}
              value={orderOptions.find((o) => o.value === section.settings.orderByField) ?? ''}
              onChange={(newValue: any) => {
                const option = newValue as OptionItem;
                const order = orderOptions.find((f) => f.value === option?.value);
                if (order) setFieldValue(`sections.${index}.settings.orderByField`, order.value);
              }}
            />
          </Col>
        </Row>
        <Show visible={section.settings.showCharts}>
          <ReportSectionCharts index={index} />
        </Show>
      </Col>
    );
  },
);
