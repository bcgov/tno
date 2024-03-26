import { Button } from 'components/button';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight } from 'react-icons/fa';
import { useFilters, useFolders, useReports } from 'store/hooks';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  getSortableOptions,
  IOptionItem,
  OptionItem,
  Row,
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
  const [{ myReports: reports }, { findMyReports }] = useReports();

  const [folderOptions, setFolderOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(folders),
  );
  const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(filters),
  );
  const [reportOptions, setReportOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(reports),
  );

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
    if (!reports.length) {
      findMyReports()
        .then((reports) => {
          setReportOptions(getSortableOptions(reports));
        })
        .catch(() => {});
    }
    // Only do on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <Col gap="0.5rem">
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
            <Col flex="1" className="description">
              <FormikSelect
                name={`sections.${index}.linkedReportId`}
                label="My Reports"
                options={reportOptions}
                value={reportOptions.find((o) => o.value === section.linkedReportId) ?? ''}
                onChange={(newValue: any) => {
                  const option = newValue as OptionItem;
                  const report = reports.find((f) => f.id === option?.value);
                  if (report) setFieldValue(`sections.${index}.linkedReport`, report);
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
    </Col>
  );
});
