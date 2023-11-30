import React from 'react';
import { useReportInstances } from 'store/hooks';
import { Col, IReportResultModel, Loading, Show } from 'tno-core';

export interface IReportInstanceViewProps {
  instanceId: number;
}

export const ReportInstanceView: React.FC<IReportInstanceViewProps> = ({ instanceId }) => {
  const [{ viewReportInstance }] = useReportInstances();

  const [isLoading, setIsLoading] = React.useState(true);
  const [preview, setPreview] = React.useState<IReportResultModel | undefined>();

  const handlePreviewReport = React.useCallback(
    async (instanceId: number) => {
      try {
        setIsLoading(true);
        const response = await viewReportInstance(instanceId);
        setPreview(response);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [viewReportInstance, setPreview],
  );

  React.useEffect(() => {
    if (instanceId) {
      handlePreviewReport(instanceId);
    }
    // The functions will result in infinite loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [instanceId]);

  return (
    <>
      <Show visible={isLoading}>
        <Loading />
        <span></span>
      </Show>
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
    </>
  );
};
