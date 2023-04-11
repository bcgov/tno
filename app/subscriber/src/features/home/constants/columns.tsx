import { Column } from 'react-table';
import { Checkbox } from 'tno-core';

import { DetermineToneIcon } from '../utils';

export const columns: Column[] = [
  {
    id: 'checkbox',
    Header: () => <Checkbox />,
    Cell: () => <Checkbox />,
    width: 0,
  },
  {
    id: 'tone',
    Header: 'TONE',
    accessor: 'tonePools',
    Cell: (cell) => <DetermineToneIcon tone={cell.value[0].value} />,
    width: 20,
  },
  {
    id: 'headline',
    Header: 'HEADLINE',
    accessor: 'headline',
    Cell: (cell) => <div className="headline">{cell.value}</div>,
  },
  {
    id: 'description',
    Header: 'SECTION PAGE',
    accessor: 'section',
    Cell: (cell) => <div className="section">{cell.value}</div>,
    width: 30,
  },
];
