import { ITimeTrackingModel } from 'hooks';
import { useLookup } from 'store/hooks';
import { GridTable, Row } from 'tno-core';

import { timeLogColumns } from './constants';
import * as styled from './styled';

export interface ITimeLogTableProps {
  /** the data to be displayed in the table */
  data: ITimeTrackingModel[];
  /** the total time logged against the content */
  totalTime: number;
}

/** Table used to display time log for users creating and updating content. */
export const TimeLogTable: React.FC<ITimeLogTableProps> = ({ data, totalTime }) => {
  const [{ users }] = useLookup();
  const parsedData = data.map((d: ITimeTrackingModel) => ({
    userName: users.find((u) => u.id === d.userId)?.displayName,
    userId: d.userId,
    activity: d.activity,
    effort: `${d.effort} Min`,
    contentId: d.contentId,
    createdOn: d.createdOn,
  }));
  return (
    <styled.TimeLogTable>
      <GridTable
        paging={{ showPaging: false }}
        columns={timeLogColumns}
        data={parsedData!!}
      ></GridTable>
      <Row>
        <p className="total-text">{`Total: ${totalTime} Min`}</p>
      </Row>
    </styled.TimeLogTable>
  );
};
