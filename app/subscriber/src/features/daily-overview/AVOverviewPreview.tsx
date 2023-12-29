import { DateFilter } from 'components/date-filter';
import parse from 'html-react-parser';
import moment from 'moment';
import React from 'react';
import { useAVOverviewInstances, useContent } from 'store/hooks';
import { Col, IReportResultModel, Loading, Show } from 'tno-core';

import * as styled from './styled';

const AVOverviewPreview: React.FC = () => {
  const [
    {
      avOverview: { filter: avOverviewFilter },
    },
    { storeAvOverviewDateFilter: storeFilter },
  ] = useContent();
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
  const handleChangeReportDate = React.useCallback(async () => {
    try {
      clear();

      const filterDate = moment(avOverviewFilter?.startDate).startOf('day').toISOString();
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
  }, [findAVOverview, viewAVOverview, avOverviewFilter]);

  React.useEffect(() => {
    if (!date) {
      getLatestPublishedOverview();
    } else if (!!date && date !== avOverviewFilter?.startDate) {
      handleChangeReportDate();
    }
  }, [date, getLatestPublishedOverview, handleChangeReportDate, avOverviewFilter]);

  React.useEffect(() => {
    if (!preview?.body) return;
    const htmlToReactElements = parse(preview?.body ?? '');
    setReactElements(htmlToReactElements);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [preview?.body]);

  return (
    <styled.AVOverviewPreview>
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Show visible={!isLoading}>
        <DateFilter filter={avOverviewFilter} storeFilter={storeFilter} />
        <Show visible={!isLoading && !!isPublished && !!reactElements}>
          <Col className="preview-report">
            <div className="danger">
              This TNO product is intended only for the use of the person to whom it is addressed.
              Please do not forward or redistribute.{' '}
            </div>
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
