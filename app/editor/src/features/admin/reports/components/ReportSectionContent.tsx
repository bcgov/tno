import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight } from 'react-icons/fa';
import { useFilters, useFolders } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  groupByOptions,
  IOptionItem,
  IReportModel,
  OptionItem,
  ReportSectionOrderByOptions,
  Row,
} from 'tno-core';

import { getSortableItems } from '../utils';

export interface IReportSectionContentProps {
  index: number;
}

export const ReportSectionContent = ({ index }: IReportSectionContentProps) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ filters }] = useFilters();
  const [{ folders }] = useFolders();

  const [filterOptions, setFilterOptions] = React.useState(getSortableItems(filters));
  const [folderOptions, setFolderOptions] = React.useState(getSortableItems(folders));
  const [orderOptions] = React.useState<IOptionItem[]>(ReportSectionOrderByOptions);

  const section = values.sections[index];
  const folder = folders.find((f) => f.id === section.folderId);
  const filter = filters.find((f) => f.id === section.filterId);

  React.useEffect(() => {
    setFilterOptions(getSortableItems(filters));
  }, [filters]);

  React.useEffect(() => {
    setFolderOptions(getSortableItems(folders));
  }, [folders]);

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
        <FormikCheckbox
          name={`sections.${index}.settings.hideEmpty`}
          label="Hide section if empty"
        />
        <FormikCheckbox
          name={`sections.${index}.settings.showHeadlines`}
          label="Show Table of Contents in this section for this section.  This is separate from the Table of Contents you can add for the whole report."
          tooltip="Display a Table of Contents at the beginning of this section."
        />
        <FormikCheckbox
          name={`sections.${index}.settings.showFullStory`}
          label="Show Full Story"
          tooltip="Display the full story for each content item in this section"
        />
        <FormikCheckbox
          name={`sections.${index}.settings.removeDuplicates`}
          label="Remove Duplicate Content"
          tooltip="Remove content from this section that is in above sections"
        />
        <FormikCheckbox
          name={`sections.${index}.settings.showImage`}
          label="Show Image"
          tooltip="Display the image for each content item in this section (if there is an image)"
        />
      </Col>
      <Col>
        <hr />
        <label>Content</label>
        <p>To automatically populate this section with content select a filter and/or a folder.</p>
        <Row gap="1rem">
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
              tooltip="Select a folder to fill this section with its content"
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
        </Row>
        <Row alignItems="center">
          <Col flex="1">
            <FormikSelect
              name={`sections.${index}.settings.sortBy`}
              label="Sort By"
              options={orderOptions}
              value={orderOptions.find((o) => o.value === section.settings.sortBy) ?? ''}
              onChange={(newValue) => {
                const option = newValue as OptionItem;
                const order = orderOptions.find((f) => f.value === option?.value);
                if (order) setFieldValue(`sections.${index}.settings.sortBy`, order.value);
              }}
            />
          </Col>
          <Checkbox
            name={`sections.${index}.settings.sortDirection`}
            label="Ascending"
            checked={section.settings.sortDirection !== 'desc'}
            onChange={(e) => {
              setFieldValue(
                `sections.${index}.settings.sortDirection`,
                e.target.checked ? 'asc' : 'desc',
              );
            }}
          />
          <Col flex="1" className="description">
            <FormikSelect
              label="Group By"
              name={`sections.${index}.settings.groupBy`}
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
    </Col>
  );
};
