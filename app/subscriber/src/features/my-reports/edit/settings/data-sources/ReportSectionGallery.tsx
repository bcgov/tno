import { DataSources } from 'features/my-reports/components';
import React from 'react';
import { useLookup } from 'store/hooks';
import { Col, FormikCheckbox, FormikSelect, OptionItem, Row, Settings, Show } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionGalleryProps {
  index: number;
}

export const ReportSectionGallery = React.forwardRef<HTMLDivElement, IReportSectionGalleryProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useReportEditContext();
    const [{ isReady, settings }] = useLookup();

    const [defaultFrontPageImagesFilterId, setFrontPageImagesFilterId] = React.useState(0);
    const [directionOptions] = React.useState([
      new OptionItem('Horizontal', 'row'),
      new OptionItem('Vertical', 'column'),
    ]);

    React.useEffect(() => {
      if (isReady) {
        const defaultFrontPageImagesFilterId = settings.find(
          (s) => s.name === Settings.FrontpageFilter,
        )?.value;
        if (defaultFrontPageImagesFilterId)
          setFrontPageImagesFilterId(+defaultFrontPageImagesFilterId);
      }
    }, [isReady, settings]);

    const section = values.sections[index];

    return (
      <Col className="frm-in">
        <DataSources
          index={index}
          className="section-options"
          none={{ value: 'none', label: 'Default Front Page Images Filter' }}
          value={section.filterId === defaultFrontPageImagesFilterId ? 'none' : undefined}
          onChange={(value) => {
            if (value === 'none')
              setFieldValue(`sections.${index}.filterId`, defaultFrontPageImagesFilterId);
          }}
        />
        <Col className="frm-in">
          <label>Report Section Options</label>
          <Show visible={!section.settings.useAllContent}>
            <Row>
              <FormikCheckbox
                name={`sections.${index}.settings.removeDuplicates`}
                label="Remove duplicate stories"
              />
              <span className="info">
                Do not include in this section content that already exists in the above sections
                (does not apply to charts that link to other reports)
              </span>
            </Row>
          </Show>
          <Show visible={!!section.folderId || !!section.linkedReportId}>
            <Row>
              <FormikCheckbox
                name={`sections.${index}.settings.overrideExcludeHistorical`}
                label={`Include all content from linked ${
                  section.folderId ? 'folder' : 'report'
                } even if in prior report`}
              />
              <span className="info">
                This overrides the report option "Exclude stories that have been sent out in
                previous report" for this section only.
              </span>
            </Row>
          </Show>
          <FormikCheckbox
            name={`sections.${index}.settings.hideEmpty`}
            label="Hide this section in the report when empty"
          />
          <FormikCheckbox
            name={`sections.${index}.settings.showImage`}
            label="Show Image"
            tooltip="Display the image for each content item in this section (if there is an image)"
          />
        </Col>
        <Col className="frm-in" flex="1">
          <FormikSelect
            name={`sections.${index}.settings.direction`}
            label="Direction of content"
            options={directionOptions}
            value={
              directionOptions.find((o) => o.value === values.sections[index].settings.direction) ??
              ''
            }
          />
        </Col>
      </Col>
    );
  },
);
