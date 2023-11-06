import { SearchWithLogout } from 'components/search-with-logout';
import React from 'react';
import { FaArrowLeft } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { useReportInstances, useReports } from 'store/hooks';
import { Col, Container, IReportModel, IReportResultModel, Row, Show } from 'tno-core';

import * as styled from './styled';

/*
  Component to view published public or personal reports. 
*/
export const ViewReport: React.FC = () => {
  const [{ viewReportInstance }] = useReportInstances();
  const [{ getReport }] = useReports();
  const navigate = useNavigate();
  const { id } = useParams();
  const [instanceId, setInstanceId] = React.useState<number>();

  const [reportView, setReportView] = React.useState<IReportResultModel>();
  const [report, setReport] = React.useState<IReportModel>();
  const [loading, setLoading] = React.useState<boolean>(false);

  React.useEffect(() => {
    getReport(parseInt(id ?? '')).then((result) => {
      setReport(result);
    });
    // just run on init
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    if (report?.instances?.length && !instanceId) setInstanceId(report.instances[0].id);
  }, [report, instanceId]);

  React.useEffect(() => {
    if (!!instanceId) {
      setLoading(true);
      viewReportInstance(instanceId)
        .then((result) => {
          setReportView(result);
          setLoading(false);
        })
        .catch(() => {});
    }
  }, [viewReportInstance, instanceId]);

  return (
    <styled.ViewReport>
      <SearchWithLogout />
      <Container className="container" isLoading={loading}>
        <Row className="header">
          <Row>
            <Row flex="1">
              <FaArrowLeft onClick={() => navigate(-1)} className="back-icon" />
              <h2 className="title">View Report</h2>
            </Row>
          </Row>
        </Row>
        <Col className="view-report">
          <Show visible={!report?.instances.length}>
            <p>There are no published version of this report.</p>
          </Show>
          <Row className="preview-header">
            <div
              className="preview-subject"
              dangerouslySetInnerHTML={{ __html: reportView?.subject ?? '' }}
            ></div>
          </Row>
          <div
            className="preview-body"
            dangerouslySetInnerHTML={{ __html: reportView?.body ?? '' }}
          ></div>
        </Col>
      </Container>
    </styled.ViewReport>
  );
};
