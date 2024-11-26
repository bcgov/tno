import { DateFilter } from 'components/date-filter';
import parse from 'html-react-parser';
import moment from 'moment';
import React from 'react';
import { useSearchParams } from 'react-router-dom';
import { useAVOverviewInstances, useContent } from 'store/hooks';
import { Col, IReportResultModel, Loader, Show } from 'tno-core';

import * as styled from './styled';

const AVOverviewPreview: React.FC = () => {
  const [
    {
      avOverview: { filter: avOverviewFilter },
    },
    { storeAvOverviewDateFilter: storeFilter },
  ] = useContent();
  const [params] = useSearchParams();
  const dateUrlParam = params.get('date');
  const [{ findAVOverview, viewAVOverview }] = useAVOverviewInstances();
  const [date, setDate] = React.useState<string>();

  const [isLoading, setIsLoading] = React.useState(true);
  const [preview, setPreview] = React.useState<IReportResultModel | undefined>();
  const [isPublished, setIsPublished] = React.useState(false);
  const [reactElements, setReactElements] = React.useState<string | JSX.Element | JSX.Element[]>();

  const clear = () => {
    setIsLoading(true);
    setPreview(undefined);
    setReactElements(undefined);
  };

  // Only used on initial load when a date hasn't been manually selected
  // Default to display the most recent published Evening Overview report
  const getLatestPublishedOverview = React.useCallback(async () => {
    try {
      clear();

      let instance = await findAVOverview();

      if (!!instance?.id) {
        // set date to latest published av overview
        const latestPublishedOnDate = moment(instance.publishedOn).startOf('day').toISOString();
        if (latestPublishedOnDate !== avOverviewFilter?.startDate) {
          setDate(latestPublishedOnDate);
          storeFilter({
            ...avOverviewFilter,
            startDate: moment(latestPublishedOnDate).add(1, 'days').startOf('day').toISOString(),
            endDate: moment(latestPublishedOnDate).add(1, 'days').endOf('day').toISOString(),
          });
        }
        const preview = await viewAVOverview(instance.id);
        setIsPublished(instance.isPublished);
        setPreview(preview);
      } else {
        // if nothing found, just set date to current date
        setDate(moment().startOf('day').toISOString());
      }
    } catch {
    } finally {
      setIsLoading(false);
    }
    // Ignoring updates to avOverviewFilter since that's handled in the below useEffect
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [findAVOverview, viewAVOverview]);

  // Used to refresh preview data for newly selected date
  const handleChangeReportDate = React.useCallback(
    async (overrideDate?: string) => {
      try {
        clear();

        const filterDate = moment(overrideDate ?? avOverviewFilter?.startDate)
          .startOf('day')
          .toISOString();
        let instance = await findAVOverview(filterDate);

        setDate(filterDate);
        if (!!instance?.id) {
          const preview = await viewAVOverview(instance.id);
          setIsPublished(instance.isPublished);
          setPreview(preview);
        }
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [findAVOverview, viewAVOverview, avOverviewFilter],
  );

  React.useEffect(() => {
    if (!date && dateUrlParam) {
      // Case 1: No date stored in memory, but there's a date parameter in path
      // in this case use that date param, set local state & fetch the overview
      setDate(moment(dateUrlParam).startOf('day').toISOString());
      storeFilter({
        ...avOverviewFilter,
        startDate: moment(dateUrlParam).startOf('day').toISOString(),
        endDate: moment(dateUrlParam).endOf('day').toISOString(),
      });
      handleChangeReportDate(dateUrlParam);
      // Case 2: No date in memory, and no date param in path, in this
      // case simply just fetch the latest published overview
    } else if (!date && !dateUrlParam) {
      getLatestPublishedOverview();
      // Case 3: There's a date in memory, but that doesn't match the date
      // which the filter is showing. In this case, fetch overview for filter
      // date and update the date in memory so it's matching.
    } else if (!!date && date !== avOverviewFilter?.startDate) {
      handleChangeReportDate();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [date, getLatestPublishedOverview, handleChangeReportDate, avOverviewFilter, dateUrlParam]);

  React.useEffect(() => {
    window.history.pushState({}, '', `?date=${moment(date).format('yyyy/MM/DD')}`);
    // we only want to update the date in the url when date in memory changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [date]);

  React.useEffect(() => {
    if (!preview?.body) return;
    const htmlToReactElements = parse(preview?.body ?? '');
    setReactElements(htmlToReactElements);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [preview?.body]);

  return (
    <styled.AVOverviewPreview>
      <Loader visible={isLoading} />
      <Show visible={!isLoading}>
        <DateFilter
          date={avOverviewFilter.startDate}
          onChangeDate={(start, end) =>
            storeFilter({
              ...avOverviewFilter,
              startDate: start,
              endDate: end,
              dateOffset: undefined,
            })
          }
        />
        <Show visible={!isLoading && !!isPublished && !!reactElements}>
          <Col className="preview-report">
            <div className="preview-body">{reactElements}</div>
          </Col>
        </Show>
      </Show>
      <Show visible={!isPublished || !reactElements}>
        No report has been published yet. Please check back later.
      </Show>
    </styled.AVOverviewPreview>
  );
};

export default AVOverviewPreview;
