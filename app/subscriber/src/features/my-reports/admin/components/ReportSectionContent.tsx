import { Button } from 'components/button';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight } from 'react-icons/fa';
import { useFilters, useFolders } from 'store/hooks';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getSortableOptions,
  IOptionItem,
  OptionItem,
  ReportSectionOrderByOptions,
  Row,
} from 'tno-core';

export interface IReportSectionContentProps {
  index: number;
}

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionContent = React.forwardRef<HTMLDivElement, IReportSectionContentProps>(
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
    const [orderOptions] = React.useState<IOptionItem[]>(ReportSectionOrderByOptions);

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
        <Row>
          <Col flex="1" className="description">
            <FormikCheckbox
              name={`sections.${index}.settings.removeDuplicates`}
              label="Remove Duplicate Content"
              tooltip="Remove content from this section that is in above sections"
            />
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
          </Col>
          <Col flex="1" className="description">
            <FormikSelect
              name={`sections.${index}.settings.sortBy`}
              label="Sort by"
              options={orderOptions}
              value={orderOptions.find((o) => o.value === section.settings.sortBy) ?? ''}
              onChange={(newValue: any) => {
                const option = newValue as OptionItem;
                const order = orderOptions.find((f) => f.value === option?.value);
                if (order) setFieldValue(`sections.${index}.settings.sortBy`, order.value);
              }}
            />
          </Col>
        </Row>
      </Col>
    );
  },
);
