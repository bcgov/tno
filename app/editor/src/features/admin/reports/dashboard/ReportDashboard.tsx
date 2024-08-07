import React from 'react';
import { FaArrowAltCircleLeft, FaArrowAltCircleRight } from 'react-icons/fa';
import { useReports } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Checkbox,
  IconButton,
  IDashboardFilter,
  IReportModel,
  ReportStatusName,
  Row,
  Text,
} from 'tno-core';

import { ReportCard } from './ReportCard';
import * as styled from './styled';

export const ReportDashboard: React.FC = () => {
  const [, { getDashboard }] = useReports();

  const [search, setSearch] = React.useState('');
  const [filter, setFilter] = React.useState<IDashboardFilter>({
    page: 1,
    quantity: 50,
    isEnabled: true,
    status: [ReportStatusName.Failed],
  });
  const [reports, setReports] = React.useState<IReportModel[]>([]);

  const fetchReports = React.useCallback(
    async (filter: IDashboardFilter) => {
      try {
        const results = await getDashboard(filter);
        setReports(results);
      } catch {}
    },
    [getDashboard],
  );

  React.useEffect(() => {
    fetchReports(filter).catch(() => {});
    // Only when filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  return (
    <styled.ReportDashboard>
      <h1>Report Dashboard</h1>
      <Row justifyContent="center" gap="1rem;">
        <Checkbox
          name="failed"
          label="Show failed only"
          checked={filter.status?.includes(ReportStatusName.Failed)}
          onChange={(e) =>
            setFilter((filter) => ({
              ...filter,
              status: e.target.checked ? [ReportStatusName.Failed] : [],
            }))
          }
        />
        <Text name="keyword" value={search ?? ''} onChange={(e) => setSearch(e.target.value)}>
          <IconButton
            iconType="search"
            onClick={(e) => setFilter((filter) => ({ ...filter, page: 1, keyword: search }))}
          />
        </Text>
      </Row>
      <div className="header">
        <div>Name</div>
        <div>Owner</div>
        <div>Status</div>
        <div>Last Sent</div>
        <div>Next Run</div>
        <div></div>
      </div>
      <div className="report-cards">
        {reports.map((report) => {
          return <ReportCard key={report.id} report={report} />;
        })}
      </div>
      <Row justifyContent="center">
        {filter.page && filter.page > 1 && (
          <Button
            variant={ButtonVariant.link}
            title="Previous"
            onClick={() => setFilter((filter) => ({ ...filter, page: filter.page! - 1 }))}
          >
            <FaArrowAltCircleLeft />
          </Button>
        )}
        <Text
          name="quantity"
          type="number"
          width="8ch"
          value={filter.quantity ?? ''}
          onChange={(e) => {
            const value = e.target.value;
            const quantity = +value;
            setFilter((filter) => ({ ...filter, page: 1, quantity }));
          }}
        />
        {filter.quantity && filter.page && filter.quantity <= reports.length && (
          <Button
            variant={ButtonVariant.link}
            title="Next"
            onClick={() => setFilter((filter) => ({ ...filter, page: filter.page! + 1 }))}
          >
            <FaArrowAltCircleRight />
          </Button>
        )}
      </Row>
    </styled.ReportDashboard>
  );
};

export default ReportDashboard;
