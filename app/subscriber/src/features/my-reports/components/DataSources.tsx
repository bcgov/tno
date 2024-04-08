import { Button } from 'components/button';
import { IToggleOption, Toggle } from 'components/form';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight } from 'react-icons/fa';
import { FaBan, FaBookmark, FaChartBar, FaFolder } from 'react-icons/fa6';
import { useFilters, useFolders, useReports } from 'store/hooks';
import {
  Col,
  FormikSelect,
  getSortableOptions,
  IOptionItem,
  OptionItem,
  ReportSectionTypeName,
  Row,
  Show,
} from 'tno-core';

import { IReportForm } from '../interfaces';

export interface IDataSourcesProps {
  /** Section index position. */
  index: number;
  /** Data source options available for this section. */
  options?: ('filter' | 'folder' | 'report' | 'none')[];
  /** The initial data source. */
  value?: 'filter' | 'folder' | 'report' | 'none';
  /** Class name of component */
  className?: string;
  /** Replace the default 'none' option with the one specified. */
  none?: IToggleOption<string>;
  /** Event fires when data source option changes. */
  onChange?: (value: string) => void;
}

/**
 * Provides a component that displays the possible data sources for this section.
 * @param param0 Component properties
 * @returns Component
 */
export const DataSources = ({
  index,
  value,
  options = [],
  className,
  none,
  onChange,
}: IDataSourcesProps) => {
  const { values, setFieldValue } = useFormikContext<IReportForm>();
  const [{ myFolders }] = useFolders();
  const [{ myFilters }] = useFilters();
  const [{ myReports }] = useReports();

  const section = values.sections[index];

  const [folderOptions, setFolderOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(myFolders),
  );
  const [filterOptions, setFilterOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(myFilters),
  );
  const [reportOptions, setReportOptions] = React.useState<IOptionItem[]>(
    getSortableOptions(myReports),
  );
  const [dataSource, setDataSource] = React.useState(
    value ??
      (section.filterId
        ? 'filter'
        : section.folderId
        ? 'folder'
        : section.linkedReportId
        ? 'report'
        : 'none'),
  );

  React.useEffect(() => {
    if (!myFolders.length) {
      setFolderOptions(getSortableOptions(myFolders));
    }
  }, [myFolders]);

  React.useEffect(() => {
    if (!myFilters.length) {
      setFilterOptions(getSortableOptions(myFilters));
    }
  }, [myFilters]);

  React.useEffect(() => {
    if (!myReports.length) {
      setReportOptions(getSortableOptions(myReports));
    }
  }, [myReports]);

  React.useEffect(() => {
    if (value) setDataSource(value);
  }, [value]);

  const toggleOptions: IToggleOption<string>[] = [];
  if (
    options.includes('filter') ||
    (!options.length &&
      [
        ReportSectionTypeName.Content,
        ReportSectionTypeName.Gallery,
        ReportSectionTypeName.MediaAnalytics,
      ].includes(section.sectionType))
  )
    toggleOptions.push({ value: 'filter', label: 'Saved Search', icon: <FaBookmark /> });
  if (
    options.includes('folder') ||
    (!options.length &&
      [
        ReportSectionTypeName.Content,
        ReportSectionTypeName.Gallery,
        ReportSectionTypeName.MediaAnalytics,
      ].includes(section.sectionType))
  )
    toggleOptions.push({ value: 'folder', label: 'Folder', icon: <FaFolder /> });
  if (
    options.includes('report') ||
    (!options.length && [ReportSectionTypeName.MediaAnalytics].includes(section.sectionType))
  )
    toggleOptions.push({ value: 'report', label: 'My Report', icon: <FaChartBar /> });
  if (
    options.includes('none') ||
    (!options.length &&
      [
        ReportSectionTypeName.Content,
        ReportSectionTypeName.Gallery,
        ReportSectionTypeName.MediaAnalytics,
      ].includes(section.sectionType))
  )
    toggleOptions.push(none ?? { value: 'none', label: 'No Data Source', icon: <FaBan /> });

  return (
    <Col gap="0.5rem" className={`data-sources${className ? ` ${className}` : ''}`}>
      <Toggle
        name="filter"
        label="Select Data Source:"
        value={dataSource}
        options={toggleOptions}
        onChange={(value) => {
          setFieldValue(`sections.${index}`, {
            ...section,
            filter: undefined,
            filterId: undefined,
            folder: undefined,
            folderId: undefined,
            linkedReport: undefined,
            linkedReportId: undefined,
          });
          setDataSource(value as any);
          onChange?.(value);
        }}
      />
      <Show visible={dataSource === 'filter'}>
        <Row className="frm-in">
          <label>My Saved Search:</label>
          <Col flex="1" className="description">
            <FormikSelect
              name={`sections.${index}.filterId`}
              options={filterOptions}
              value={filterOptions.find((o) => o.value === section.filterId) ?? ''}
              onChange={(newValue: any) => {
                const option = newValue as OptionItem;
                const filter = myFilters.find((f) => f.id === option?.value);
                if (filter) setFieldValue(`sections.${index}.filter`, filter);
              }}
            >
              <Button
                disabled={!values.sections[index].filterId}
                onClick={() =>
                  window.open(`/search/advanced/${values.sections[index].filterId}`, '_blank')
                }
              >
                View
                <FaArrowAltCircleRight />
              </Button>
            </FormikSelect>
            {section.filter?.description && <p>{section.filter?.description}</p>}
          </Col>
        </Row>
      </Show>
      <Show visible={dataSource === 'folder'}>
        <Row className="frm-in">
          <label>My folder:</label>
          <Col flex="1" className="description">
            <FormikSelect
              name={`sections.${index}.folderId`}
              options={folderOptions}
              value={folderOptions.find((o) => o.value === section.folderId) ?? ''}
              onChange={(newValue: any) => {
                const option = newValue as OptionItem;
                const folder = myFolders.find((f) => f.id === option?.value);
                if (folder) setFieldValue(`sections.${index}.folder`, folder);
              }}
            >
              <Button
                disabled={!values.sections[index].folderId}
                onClick={() => window.open(`/folders/${values.sections[index].folderId}`, '_blank')}
              >
                View
                <FaArrowAltCircleRight />
              </Button>
            </FormikSelect>
            {section.folder?.description && <p>{section.folder?.description}</p>}
          </Col>
        </Row>
      </Show>
      <Show visible={dataSource === 'report'}>
        <Row className="frm-in">
          <label>My Report:</label>
          <Col flex="1" className="description">
            <FormikSelect
              name={`sections.${index}.linkedReportId`}
              options={reportOptions}
              value={reportOptions.find((o) => o.value === section.linkedReportId) ?? ''}
              onChange={(newValue: any) => {
                const option = newValue as OptionItem;
                const report = myReports.find((r) => r.id === option?.value);
                if (report) setFieldValue(`sections.${index}.linkedReport`, report);
              }}
            >
              <Button
                disabled={!values.sections[index].linkedReportId}
                onClick={() =>
                  window.open(`/reports/${values.sections[index].linkedReportId}/content`, '_blank')
                }
              >
                View
                <FaArrowAltCircleRight />
              </Button>
            </FormikSelect>
            {section.linkedReport?.description && <p>{section.linkedReport?.description}</p>}
          </Col>
        </Row>
      </Show>
    </Col>
  );
};
