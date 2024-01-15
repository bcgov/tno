import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import { GridTable, ITimeTrackingModel, Row } from 'tno-core';

import { ITimeTrackingForm } from './components/time-log/interfaces';
import { timeLogColumns } from './constants';
import { IContentForm } from './interfaces';
import * as styled from './styled';

/** Table used to display time log for users creating and updating content. */
export const TimeLogTable: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [{ users }] = useLookup();

  const parsedData = values.timeTrackings.map<ITimeTrackingForm>((d: ITimeTrackingModel) => ({
    id: d.id,
    userName: users.find((u) => u.id === d.userId)?.displayName ?? '',
    userId: d.userId,
    activity: d.activity,
    effort: d.effort,
    contentId: d.contentId,
    createdOn: d.createdOn,
  }));

  const effort = values.timeTrackings.reduce((result, entry) => result + entry.effort, 0);

  return (
    <styled.TimeLogTable>
      <GridTable
        paging={{ pageSizeOptions: { show: false } }}
        columns={timeLogColumns(setFieldValue, values)}
        data={parsedData}
      ></GridTable>
      <Row>
        <p className="total-text">{`Total: ${effort} Min`}</p>
      </Row>
    </styled.TimeLogTable>
  );
};
