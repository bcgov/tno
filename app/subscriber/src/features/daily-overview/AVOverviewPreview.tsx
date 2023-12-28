import { DateFilter } from 'components/date-filter';
import parse from 'html-react-parser';
import moment from 'moment';
import React from 'react';
import { useAVOverviewInstances, useContent } from 'store/hooks';
import {
  Col,
  IAVOverviewInstanceModel,
  IFilterModel,
  IReportResultModel,
  Loading,
  Show,
} from 'tno-core';

import { defaultFilter } from './constants/defaultFilter';
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
  const [, setInstance] = React.useState<IAVOverviewInstanceModel>();
  const [preview, setPreview] = React.useState<IReportResultModel | undefined>();
  const [isPublished, setIsPublished] = React.useState(false);
  const [reactElements, setReactElements] = React.useState<string | JSX.Element | JSX.Element[]>();

  const getLatestPublishedOverview = React.useCallback(async () => {
    try {
      setIsLoading(true);
      let instance = await findAVOverview();
      console.log('**************initial load instance ******************');
      console.log(instance);
      console.log('*************initial load instance *******************');
      if (!!instance?.id) {
        const latestPublishedOnDate = moment(instance.publishedOn).startOf('day').toISOString();
        setDate(latestPublishedOnDate);
        if (latestPublishedOnDate !== avOverviewFilter?.startDate) {
          console.log('**************store filter 2****************');
          storeFilter({
            ...avOverviewFilter,
            startDate: moment(latestPublishedOnDate).add(1, 'days').startOf('day').toISOString(),
            endDate: moment(latestPublishedOnDate).add(1, 'days').endOf('day').toISOString(),
          });
        }

        const preview = await viewAVOverview(instance.id);
        setIsPublished(instance.isPublished);
        setPreview(preview);
      }
    } catch {
    } finally {
      setIsLoading(false);
    }
  }, [findAVOverview, viewAVOverview]);

  const handlePreviewReport = React.useCallback(async () => {
    try {
      setIsLoading(true);
      setPreview(undefined);
      setReactElements(undefined);
      console.log('**********query for *******************');
      console.log(moment(avOverviewFilter?.startDate).startOf('day').toISOString());
      console.log('**********query for *******************');
      let instance = await findAVOverview(
        moment(avOverviewFilter?.startDate).startOf('day').toISOString(),
      );
      setDate(moment(avOverviewFilter?.startDate).startOf('day').toISOString());
      console.log('**************instance******************');
      console.log(instance);
      console.log('*************instance*******************');
      setInstance(instance);
      if (!!instance?.id) {
        const preview = await viewAVOverview(instance.id);
        console.log('**************preview ******************');
        console.log(preview);
        console.log('*************preview *******************');
        setIsPublished(instance.isPublished);
        setPreview(preview);
      }
    } catch {
    } finally {
      setIsLoading(false);
    }
  }, [findAVOverview, viewAVOverview, avOverviewFilter]);

  // React.useEffect(() => {
  //   if (!firstLoad) {
  //     handlePreviewReport();
  //   }
  // }, [handlePreviewReport]);

  React.useEffect(() => {
    if (!preview?.body) return;
    const htmlToReactElements = parse(preview?.body ?? '');
    setReactElements(htmlToReactElements);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [preview?.body]);

  React.useEffect(() => {
    if (!date) {
      getLatestPublishedOverview();
    } else if (!!date && date !== avOverviewFilter?.startDate) {
      handlePreviewReport();
    }
  }, [date, getLatestPublishedOverview, handlePreviewReport, avOverviewFilter]);

  return (
    <styled.AVOverviewPreview>
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Show visible={!isLoading && !!isPublished && !!reactElements}>
        <DateFilter filter={avOverviewFilter} storeFilter={storeFilter} />
        <Col className="preview-report">
          <div className="danger">
            This TNO product is intended only for the use of the person to whom it is addressed.
            Please do not forward or redistribute.{' '}
          </div>
          <div className="preview-body">{reactElements}</div>
        </Col>
      </Show>
      <Show visible={!isPublished || !reactElements}>
        No report has been published yet. Please check back later.
      </Show>
    </styled.AVOverviewPreview>
  );
};

export default AVOverviewPreview;
