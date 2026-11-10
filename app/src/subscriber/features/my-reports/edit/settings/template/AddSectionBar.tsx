import { Button } from 'components/button';
import { defaultReportSection } from 'features/my-reports/constants';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { BiSolidFileJson } from 'react-icons/bi';
import {
  FaAlignJustify,
  FaBrain,
  FaChartPie,
  FaImage,
  FaImages,
  FaList,
  FaNewspaper,
} from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { ReportSectionTypeName, Row, Settings } from 'tno-core';

import * as styled from './styled';

export const AddSectionBar = () => {
  const { setFieldValue, values, isSubmitting } = useFormikContext<IReportForm>();
  const [{ isReady, settings }] = useLookup();

  const [defaultFrontPageImagesFilterId, setFrontPageImagesFilterId] = React.useState(0);

  React.useEffect(() => {
    if (isReady) {
      const defaultFrontPageImagesFilterId = settings.find(
        (s) => s.name === Settings.FrontpageFilter,
      )?.value;
      if (defaultFrontPageImagesFilterId)
        setFrontPageImagesFilterId(+defaultFrontPageImagesFilterId);
      else toast.error(`Configuration settings '${Settings.FrontpageFilter}' is required.`);
    }
  }, [isReady, settings]);

  const addSection = React.useCallback(
    (
      index: number,
      type: ReportSectionTypeName,
      showHeadlines: boolean | undefined = undefined,
      showFullStory: boolean | undefined = undefined,
    ) => {
      const newSection = defaultReportSection(
        type,
        index === 0
          ? 0
          : index < values.sections.length
          ? values.sections[index].sortOrder + 1
          : values.sections[values.sections.length - 1].sortOrder,
        showHeadlines,
        showFullStory,
        values.hideEmptySections,
      );
      const sections = [...values.sections];
      // A Gallery will default to a front page images section.
      sections.splice(index, 0, {
        ...newSection,
        filterId:
          type === ReportSectionTypeName.Gallery ? defaultFrontPageImagesFilterId : undefined,
      });
      setFieldValue(
        'sections',
        sections.map((s, i) => ({ ...s, sortOrder: i })),
      );
    },
    [setFieldValue, values, defaultFrontPageImagesFilterId],
  );

  return (
    <styled.AddSectionBar className="section-bar">
      <Row>Click to add report sections:</Row>
      <Row>
        <Button
          onClick={() => addSection(0, ReportSectionTypeName.TableOfContents)}
          disabled={
            isSubmitting ||
            values.sections.some((s) => s.sectionType === ReportSectionTypeName.TableOfContents)
          }
        >
          <Row gap="1rem">
            <FaList />
            <label>Table of Contents</label>
          </Row>
        </Button>
        <Button
          onClick={() => addSection(values.sections.length, ReportSectionTypeName.AI, false, false)}
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaBrain />
            <label>AI</label>
          </Row>
        </Button>
        <Button
          onClick={() =>
            addSection(values.sections.length, ReportSectionTypeName.Content, false, true)
          }
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaNewspaper />
            <label>Stories</label>
          </Row>
        </Button>
        <Button
          onClick={() =>
            addSection(values.sections.length, ReportSectionTypeName.MediaAnalytics, false, false)
          }
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaChartPie />
            <label>Media Analytics</label>
          </Row>
        </Button>
        <Button
          onClick={() => addSection(values.sections.length, ReportSectionTypeName.Text)}
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaAlignJustify />
            <label>Text</label>
          </Row>
        </Button>
        <Button
          onClick={() =>
            addSection(values.sections.length, ReportSectionTypeName.Gallery, false, false)
          }
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaImages />
            <label>Front Page Images</label>
          </Row>
        </Button>
        <Button
          onClick={() =>
            addSection(values.sections.length, ReportSectionTypeName.Image, false, false)
          }
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <FaImage />
            <label>Image</label>
          </Row>
        </Button>
        <Button
          onClick={() =>
            addSection(values.sections.length, ReportSectionTypeName.Data, false, false)
          }
          disabled={isSubmitting}
        >
          <Row gap="1rem">
            <BiSolidFileJson />
            <label>Data</label>
          </Row>
        </Button>
      </Row>
    </styled.AddSectionBar>
  );
};
