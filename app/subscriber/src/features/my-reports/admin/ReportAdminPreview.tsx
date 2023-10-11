import { useFormikContext } from 'formik';
import React from 'react';
import { FaEdit, FaEye, FaPlay, FaSyncAlt, FaTelegramPlane } from 'react-icons/fa';
import { FaGear } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { useReports } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Button, ButtonVariant, Col, Container, IReportResultModel, Row, Show } from 'tno-core';

import { IReportForm } from '../interfaces';

export const ReportAdminPreview: React.FC = () => {
  const { values, isSubmitting } = useFormikContext<IReportForm>();
  const navigate = useNavigate();
  const [{ previewReport }] = useReports();
  const [{ requests }] = useAppStore();

  const [preview, setPreview] = React.useState<IReportResultModel>();

  const handleRefresh = React.useCallback(
    async (report: IReportForm) => {
      try {
        const result = await previewReport(report.id);
        setPreview(result);
      } catch {}
    },
    [previewReport],
  );

  return (
    <Col>
      <Container isLoading={requests.some((r) => r.url.includes('preview-report'))}>
        <Row className="header">
          <Row flex="1">
            <FaEye size={20} />
            <Row flex="1">
              <h2>Preview</h2>
            </Row>
            <Button
              onClick={() => handleRefresh(values)}
              disabled={isSubmitting || !values.id}
              title="Refresh"
            >
              <FaSyncAlt />
            </Button>
            <Show visible={!!values.id}>
              <Button
                variant={ButtonVariant.secondary}
                disabled={isSubmitting}
                onClick={() => navigate(`/reports/${values.id}/edit`)}
                title="Edit"
              >
                <FaEdit className="btn btn-link" />
              </Button>
            </Show>
          </Row>
        </Row>
        <Col className="preview-report">
          <Show visible={!preview}>
            <Col className="diagram" alignContent="center">
              <Row nowrap>
                <Col alignItems="center">
                  <FaGear size={30} />
                  <h3>Configure</h3>
                </Col>
                <Col justifyContent="center">
                  <p>Create and configure your report. Add sections with content.</p>
                </Col>
              </Row>
              <Row nowrap>
                <Col alignItems="center">
                  <FaEye size={30} />
                  <h3>Preview</h3>
                </Col>
                <Col justifyContent="center">
                  <p>Preview an example of what your report looks like.</p>
                  <p>Save your report when you are happy with the appearance.</p>
                </Col>
              </Row>
              <Row nowrap>
                <Col alignItems="center">
                  <FaPlay size={30} />
                  <h3>Run</h3>
                </Col>
                <Col justifyContent="center">
                  <p>
                    When a report is run it will generate a snapshot. This represents a moment in
                    time and adds content to the report.
                  </p>
                </Col>
              </Row>
              <Row nowrap>
                <Col alignItems="center">
                  <FaEdit size={30} />
                  <h3>Edit</h3>
                </Col>
                <Col justifyContent="center">
                  <p>
                    Before sending out a report, edit the snapshot and manually add/remove content.
                  </p>
                </Col>
              </Row>
              <Row nowrap>
                <Col alignItems="center">
                  <FaTelegramPlane size={30} />
                  <h3>Send</h3>
                </Col>
                <Col justifyContent="center">
                  <p>Send your report out by email to subscribers.</p>
                </Col>
              </Row>
            </Col>
          </Show>
          <Show visible={!!preview}>
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
          </Show>
        </Col>
      </Container>
    </Col>
  );
};
