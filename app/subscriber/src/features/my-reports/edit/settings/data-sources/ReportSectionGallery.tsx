import { DataSources } from 'features/my-reports/components';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import { Col, FormikCheckbox, Settings } from 'tno-core';

export interface IReportSectionGalleryProps {
  index: number;
}

export const ReportSectionGallery = React.forwardRef<HTMLDivElement, IReportSectionGalleryProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useFormikContext<IReportForm>();
    const [{ isReady, settings }] = useLookup();

    const [defaultFrontPageImagesFilterId, setFrontPageImagesFilterId] = React.useState(0);

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
          <FormikCheckbox
            name={`sections.${index}.settings.hideEmpty`}
            label="Hide this section in the report when empty"
          />
        </Col>
      </Col>
    );
  },
);
