import { GridTable } from 'components/grid-table';
import { Row } from 'components/row';
import { ITimeTrackingModel } from 'hooks';
import { useLookup } from 'store/hooks';
import styled from 'styled-components';

import { timeLogColumns } from './constants';

export interface ITimeLogTableProps {
  /** the data to be displayed in the table */
  data: ITimeTrackingModel[];
  /** the total time logged against the content */
  totalTime: number;
}

const TableContainer = styled.div`
  div[role='table'] {
    width: 550px;
  }
  .total-text {
    font-weight: 700;
    margin-top: 1%;
    margin-left: 1%;
  }
`;

/** Table used to display time log for users creating and updating content. */
export const TimeLogTable: React.FC<ITimeLogTableProps> = ({ data, totalTime }) => {
  const [{ users }] = useLookup();
  console.log(data, 'data');
  const parsedData = data.map((d: ITimeTrackingModel) => ({
    userName: users.find((u) => u.id === d.userId)?.displayName,
    userId: d.userId,
    activity: d.activity,
    effort: `${d.effort} Min`,
    contentId: d.contentId,
    createdOn: d.createdOn,
  }));
  return (
    <TableContainer>
      <GridTable
        paging={{ showPaging: false }}
        columns={timeLogColumns}
        data={parsedData!!}
      ></GridTable>
      <Row>
        <p className="total-text">{`Total: ${totalTime} Min`}</p>
      </Row>
    </TableContainer>
  );
};
