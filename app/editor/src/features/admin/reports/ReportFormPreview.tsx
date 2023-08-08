import { AxiosError } from 'axios';
import React from 'react';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, IReportModel, Row, Text } from 'tno-core';

import { useReportTemplateContext } from './ReportTemplateContext';

export interface IReportFormPreviewProps {}

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormPreview: React.FC<IReportFormPreviewProps> = () => {
  const { values, preview, setPreview } = useReportTemplateContext();
  const [, { sendReport, previewReport }] = useReports();

  const [sendTo, setSendTo] = React.useState('');

  const handleSend = async (values: IReportModel, to: string) => {
    try {
      await sendReport(values, to);
      toast.success('Report has been successfully requested');
    } catch {}
  };

  const handlePreviewReport = React.useCallback(
    async (model: IReportModel) => {
      try {
        const response = await previewReport({
          ...model,
          instances: [],
          subscribers: [],
          owner: undefined,
        });
        setPreview(response);
      } catch (ex) {
        const error = ex as AxiosError;
        const response = error.response;
        const data = response?.data as any;
        setPreview({
          subject: data.error,
          body: `${data.details}<div>${data.stackTrace}</div>`,
          results: {},
        });
      }
    },
    [previewReport, setPreview],
  );

  return (
    <>
      <h2>{values.name}</h2>
      <Row>
        <Col flex="1" alignItems="center" justifyContent="center">
          <p>
            Before saving the report, generate a preview to ensure it is working and returning the
            correct content. Previewed reports must have a filter. When testing a custom report
            change it temporarily to a filter.
          </p>
          <Button variant={ButtonVariant.success} onClick={() => handlePreviewReport(values)}>
            Generate Preview
          </Button>
        </Col>
        {values.id && (
          <Col flex="1">
            <p>After you save the report, send a test email to the following address.</p>
            <Text
              name="to"
              label="Email To"
              value={sendTo}
              onChange={(e) => setSendTo(e.target.value)}
            >
              <Button
                variant={ButtonVariant.secondary}
                disabled={!sendTo}
                onClick={async () => await handleSend(values, sendTo)}
              >
                Send
              </Button>
            </Text>
          </Col>
        )}
      </Row>
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
    </>
  );
};
