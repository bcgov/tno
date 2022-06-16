import { faTrash } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { useFormikContext } from 'formik';
import { IContentModel, ITimeTrackingModel } from 'hooks';
import React from 'react';
import { Column } from 'react-table';
import { useLookup } from 'store/hooks';
import { Center, GridTable, Row } from 'tno-core';

import * as styled from './styled';
import { getTotalTime } from './utils';

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
  // const [totalEffort, setTotalEffort] = React.useState(totalTime);
  const parsedData = data.map((d: ITimeTrackingModel) => ({
    userName: users.find((u) => u.id === d.userId)?.displayName,
    userId: d.userId,
    activity: d.activity,
    effort: `${d.effort} Min`,
    contentId: d.contentId,
    createdOn: d.createdOn,
  }));

  const { values, setFieldValue } = useFormikContext<IContentModel>();

  /** columns located within file for state manipulation */
  const timeLogColumns: Column<ITimeTrackingModel>[] = [
    {
      id: 'effort',
      Header: () => <Center>TIME</Center>,
      accessor: 'effort',
      Cell: ({ value }) => <Center>{value}</Center>,
    },
    {
      id: 'user',
      Header: () => <Center>USER</Center>,
      accessor: 'userName',
      Cell: ({ value }) => <Center>{value}</Center>,
    },
    {
      id: 'activity',
      Header: () => <Center>TASK</Center>,
      accessor: 'activity',
      Cell: ({ value }) => <Center>{value}</Center>,
    },
    {
      id: 'content',
      Header: () => <Center>DATE</Center>,
      accessor: 'createdOn',
      Cell: ({ value }: any) => {
        const date = new Date(value);
        return <Center>{`${date.getMonth()}/${date.getDate()}/${date.getFullYear()}`}</Center>;
      },
    },
    {
      id: 'actions',
      Header: () => <Center>ACTION</Center>,
      Cell: ({ row, data }: any) => (
        <Center>
          <FontAwesomeIcon
            onClick={() => {
              values.timeTrackings?.splice(row.id, 1);
              setFieldValue('timeTrackings', values.timeTrackings);
              setTotalEffort(!!values.timeTrackings ? getTotalTime(values.timeTrackings) : 0);
            }}
            icon={faTrash}
          />
        </Center>
      ),
    },
  ];

  return (
    <styled.TimeLogTable>
      <GridTable
        paging={{ showPaging: false }}
        columns={timeLogColumns}
        data={parsedData!!}
      ></GridTable>
      <Row>
        <p className="total-text">{`Total: ${totalEffort} Min`}</p>
      </Row>
    </styled.TimeLogTable>
  );
};
