import React from 'react';
import { useAVOverviewInstances } from 'store/hooks';
import { Col, IAVOverviewInstanceModel, IReportResultModel, Loading, Show } from 'tno-core';

import * as styled from './styled';

const AVOverviewPreview: React.FC = () => {
  const [{ findAVOverview, viewAVOverview }] = useAVOverviewInstances();

  const [isLoading, setIsLoading] = React.useState(true);
  const [, setInstance] = React.useState<IAVOverviewInstanceModel>();
  const [preview, setPreview] = React.useState<IReportResultModel | undefined>();
  const [isPublished, setIsPublished] = React.useState(false);

  const handlePreviewReport = React.useCallback(async () => {
    try {
      setIsLoading(true);
      const instance = await findAVOverview(new Date().toISOString());
      setInstance(instance);
      if (!!instance?.id) {
        const preview = await viewAVOverview(instance?.id ?? 0);
        setIsPublished(instance?.isPublished ?? false);
        setPreview(preview);
      }
    } catch {
    } finally {
      setIsLoading(false);
    }
  }, [findAVOverview, viewAVOverview]);

  React.useEffect(() => {
    handlePreviewReport();
  }, [handlePreviewReport]);

  return (
    <styled.AVOverviewPreview>
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Show visible={!isLoading && !!isPublished}>
        <Col className="preview-report">
          <div className="danger">
            This TNO product is intended only for the use of the person to whom it is addressed.
            Please do not forward or redistribute.{' '}
          </div>
          <div
            className="preview-body"
            dangerouslySetInnerHTML={{ __html: preview?.body ?? '' }}
          ></div>
        </Col>
      </Show>
      <Show visible={!isPublished}>No report has been published yet. Please check back later.</Show>
    </styled.AVOverviewPreview>
  );
};

export default AVOverviewPreview;
