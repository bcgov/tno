import { useFormikContext } from 'formik';
import React from 'react';
import { FaEye, FaSyncAlt, FaTelegramPlane } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useReportInstances } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Button, ButtonVariant, Col, Container, IReportResultModel, Row, Text } from 'tno-core';

import { IReportForm } from '../interfaces';

export const ReportSnapshotView: React.FC = () => {
  const { values, isSubmitting, setFieldValue } = useFormikContext<IReportForm>();
  const [{ viewReportInstance, sendReportInstance }] = useReportInstances();
  const [{ requests }] = useAppStore();

  const [preview, setPreview] = React.useState<IReportResultModel>();
  const [to, setTo] = React.useState('');

  const instanceId = values.instances.length ? values.instances[0].id : undefined;

  React.useEffect(() => {
    if (instanceId)
      viewReportInstance(instanceId)
        .then((result) => {
          setPreview(result);
        })
        .catch(() => {});
  }, [viewReportInstance, instanceId]);

  const handleRefresh = React.useCallback(
    async (instanceId: number) => {
      try {
        const result = await viewReportInstance(instanceId);
        setPreview(result);
      } catch {}
    },
    [viewReportInstance],
  );

  const handleSend = React.useCallback(
    async (id: number, to: string) => {
      try {
        const result = await sendReportInstance(id, to);
        setFieldValue('instances.0', result);
        toast.success('Report has been submitted.');
      } catch {}
    },
    [sendReportInstance, setFieldValue],
  );

  return (
    <Col>
      <Container isLoading={requests.some((r) => r.url.includes('preview-report-instance'))}>
        <Row className="header">
          <Row>
            <FaEye size={20} />
            <Col flex="1">
              <h2>View Report</h2>
            </Col>
          </Row>
          <Row>
            <Button
              onClick={() => instanceId && handleRefresh(instanceId)}
              disabled={isSubmitting}
              title="Refresh"
            >
              <FaSyncAlt />
            </Button>
            <label>Send to:</label>
            <Text name="email" value={to} onChange={(e) => setTo(e.target.value)}>
              <Button
                variant={ButtonVariant.success}
                disabled={isSubmitting || !to.length}
                title="Send now"
                onClick={() => !!instanceId && handleSend(instanceId, to)}
              >
                <Row gap="0.25rem" alignItems="center" nowrap>
                  <FaTelegramPlane />
                </Row>
              </Button>
            </Text>
          </Row>
        </Row>
        <Col className="preview-report">
          <Row className="preview-header">
            <div
              className="preview-subject"
              dangerouslySetInnerHTML={{ __html: preview?.subject ?? '' }}
            ></div>
          </Row>
          <div
            className="preview-body"
            dangerouslySetInnerHTML={{ __html: preview?.body ?? '' }}
          ></div>
        </Col>
      </Container>
    </Col>
  );
};
