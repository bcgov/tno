import { DateFilter } from 'components/date-filter';
import React from 'react';
import { useParams } from 'react-router-dom';
import { useContent, useReports } from 'store/hooks';
import { Col, IReportResultModel, Loading, Show } from 'tno-core';

import * as styled from './styled';

const EventOfTheDayPreview: React.FC = () => {
  const [
    {
      eventOfTheDay: { filter: eventOfTheDayFilter },
    },
    { storeEventofTheDayDateFilter: storeFilter },
  ] = useContent();
  const [, { previewReport, findInstancesForReportId }] = useReports();
  const reportId = 8;

  const [isLoading, setIsLoading] = React.useState(true);
  const [preview, setPreview] = React.useState<IReportResultModel | undefined>();

  const handlePreviewReport = React.useCallback(
    async (reportId: number) => {
      try {
        setIsLoading(true);
        // const response = await previewReport(reportId);
        // setPreview(response);
        const instances = await findInstancesForReportId(reportId);
        console.log('testinstances', instances);
        setPreview(instances[0]);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [findInstancesForReportId],
  );

  React.useEffect(() => {
    handlePreviewReport(reportId);
  }, []);
  return (
    <styled.EventOfTheDayPreview>
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Show visible={!isLoading}>
        <DateFilter filter={eventOfTheDayFilter} storeFilter={storeFilter} />
        <Show visible={!isLoading}>
          <Col className="preview-report">
            <div
              className="preview-subject"
              dangerouslySetInnerHTML={{ __html: preview?.subject ?? '' }}
            ></div>
            <div
              className="preview-body"
              dangerouslySetInnerHTML={{ __html: preview?.body ?? '' }}
            ></div>
          </Col>
        </Show>
      </Show>
      {/* <Show visible={!isPublished || !reactElements}>
        No report has been published yet. Please check back later.
      </Show> */}
    </styled.EventOfTheDayPreview>
  );
};

export default EventOfTheDayPreview;
