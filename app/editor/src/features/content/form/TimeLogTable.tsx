import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import { GridTable, IContentModel, ITimeTrackingModel, Row } from 'tno-core';

import { timeLogColumns } from './constants';
import * as styled from './styled';

export interface ITimeLogTableProps {
  /** the data to be displayed in the table */
  data: ITimeTrackingModel[];
  /** the total time logged against the content */
  totalEffort: number;
  setTotalEffort: (effort: number) => void;
}

/** Table used to display time log for users creating and updating content. */
export const TimeLogTable: React.FC<ITimeLogTableProps> = ({
  data,
  totalEffort,
  setTotalEffort,
}) => {
  const [{ users }] = useLookup();
  const parsedData = data.map((d: ITimeTrackingModel) => ({
    userName: users.find((u) => u.id === d.userId)?.displayName,
    userId: d.userId,
    activity: d.activity,
    effort: `${d.effort} Min`,
    contentId: d.contentId,
    createdOn: d.createdOn,
  }));

  const { values, setFieldValue } = useFormikContext<IContentModel>();

  return (
    <styled.TimeLogTable>
      <GridTable
        paging={{ showPaging: false }}
        columns={timeLogColumns(setTotalEffort, setFieldValue, values)}
        data={parsedData}
      ></GridTable>
      <Row>
        <p className="total-text">{`Total: ${totalEffort} Min`}</p>
      </Row>
    </styled.TimeLogTable>
  );
};
