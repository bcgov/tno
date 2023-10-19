import { SearchWithLogout } from 'components/search-with-logout';
import React from 'react';
import { FaArrowLeft } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { useReports } from 'store/hooks';
import { Col, FlexboxTable, IReportInstanceModel, Row } from 'tno-core';

import { columns } from '../view/constants';
import * as styled from './styled';

export const ReportInstances: React.FC = () => {
  const navigate = useNavigate();
  const [{ findInstancesForReportId }] = useReports();

  const { id } = useParams();

  const [instances, setInstances] = React.useState<IReportInstanceModel[]>([]);

  React.useEffect(() => {
    if (!!id) {
      findInstancesForReportId(Number(id)).then((instances) => {
        setInstances(instances);
      });
    }
    // Only load the report instances when clicking on the tab.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handlePreview = React.useCallback((model: IReportInstanceModel) => {
    window.open(`/report/instances/${model.id}/preview`, '_blank');
  }, []);

  return (
    <styled.ReportInstances>
      <SearchWithLogout />
      <Col className="my-reports">
        <Row className="header-row">
          <FaArrowLeft className="back-arrow" onClick={() => navigate(-1)} />
          <div className="title">My report instances </div>
        </Row>
        <FlexboxTable
          pagingEnabled={false}
          columns={columns({
            onPreview: handlePreview,
          })}
          rowId={'id'}
          data={instances}
          showActive={false}
        />
      </Col>
    </styled.ReportInstances>
  );
};
