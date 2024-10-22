import { DateFilter } from 'components/date-filter';
import moment from 'moment';
import React from 'react';
import { useSearchParams } from 'react-router-dom';
import { useContent, useReportInstances, useSettings } from 'store/hooks';
import { Col, IReportResultModel, Loader, Show } from 'tno-core';

import * as styled from './styled';

const EventOfTheDayPreview: React.FC = () => {
  const [
    {
      eventOfTheDay: { filter: eventOfTheDayFilter },
    },
    { storeEventofTheDayDateFilter: storeFilter },
  ] = useContent();
  const [params] = useSearchParams();
  const dateUrlParam = params.get('date')
    ? params.get('date')
    : moment(new Date()).endOf('day').toISOString();
  const [{ getReportInstance }] = useReportInstances();
  const { eventOfTheDayReportId } = useSettings();
  const [date, setDate] = React.useState<string>();

  const [isLoading, setIsLoading] = React.useState(false);
  const [preview, setPreview] = React.useState<IReportResultModel | undefined>();

  // Used to refresh preview data for newly selected date
  const handleChangeReportDate = React.useCallback(
    async (eventOfTheDayReportId: number, overrideDate?: string) => {
      try {
        setIsLoading(true);
        const filterDate = moment(overrideDate ?? eventOfTheDayFilter?.startDate)
          .startOf('day')
          .toISOString();

        setDate(filterDate);
        const instance = await getReportInstance(eventOfTheDayReportId, false, filterDate);
        setPreview(instance);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [eventOfTheDayFilter?.startDate, getReportInstance],
  );

  React.useEffect(() => {
    if (eventOfTheDayReportId === undefined) return;
    if (!date && dateUrlParam) {
      // Case 1: No date stored in memory, but there's a date parameter in path
      // in this case use that date param, set local state & fetch the overview
      setDate(moment(dateUrlParam).startOf('day').toISOString());
      storeFilter({
        ...eventOfTheDayFilter,
        startDate: moment(dateUrlParam).startOf('day').toISOString(),
        endDate: moment(dateUrlParam).endOf('day').toISOString(),
      });
      handleChangeReportDate(eventOfTheDayReportId, dateUrlParam);
      // Case 2: There's a date in memory, but that doesn't match the date
      // which the filter is showing. In this case, fetch overview for filter
      // date and update the date in memory so it's matching.
    } else if (!!date && date !== eventOfTheDayFilter?.startDate) {
      handleChangeReportDate(eventOfTheDayReportId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [handleChangeReportDate, eventOfTheDayFilter, dateUrlParam, eventOfTheDayReportId]);

  React.useEffect(() => {
    window.history.pushState({}, '', `?date=${moment(date).format('yyyy/MM/DD')}`);
    // we only want to update the date in the url when date in memory changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [date]);

  return (
    <styled.EventOfTheDayPreview>
      <Loader visible={isLoading} />
      <Show visible={!isLoading}>
        <DateFilter filter={eventOfTheDayFilter} storeFilter={storeFilter} />
        <Show visible={!isLoading && !!preview}>
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
      <Show visible={!isLoading && !preview}>
        No report has been published yet. Please check back later.
      </Show>
    </styled.EventOfTheDayPreview>
  );
};

export default EventOfTheDayPreview;
