import { IContentModel, ITableHookColumn } from 'tno-core';

import { DetermineToneIcon } from '../utils';

export const columns: ITableHookColumn<IContentModel>[] = [
  {
    name: 'tone',
    label: 'TONE',
    cell: (cell: any) => <DetermineToneIcon tone={cell.original.tonePools[0]?.value ?? 0} />,
  },
  {
    name: 'headline',
    label: 'HEADLINE',
    cell: (cell) => <div className="headline">{cell?.original.headline}</div>,
    width: 5,
  },
  {
    name: 'sectionPage',
    label: 'SECTION PAGE',
    cell: (cell: any) => (
      <div className="section">{`${cell?.original.section}:${cell?.original.page}`}</div>
    ),
    width: 1,
  },
];
