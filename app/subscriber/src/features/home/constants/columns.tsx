import { ContentTypeName, IContentModel, ITableHookColumn } from 'tno-core';

import { DetermineToneIcon } from '../utils';

export const determinecolumns = (contentType: ContentTypeName) => {
  let cols: ITableHookColumn<IContentModel>[] = [];
  if (contentType === ContentTypeName.PrintContent) {
    cols = [
      {
        name: 'tone',
        label: 'TONE',
        cell: (cell) => (
          <DetermineToneIcon
            tone={cell.original.tonePools ? cell.original.tonePools[0]?.value : 0}
          />
        ),
      },
      {
        name: 'headline',
        label: 'HEADLINE',
        cell: (cell) => <div className="headline">{cell.original.headline}</div>,
        width: 5,
      },
      {
        name: 'sectionPage',
        label: 'SECTION PAGE',
        cell: (cell) => (
          <div className="section">{`${cell.original.section}: ${cell?.original.page}`}</div>
        ),
        width: 1,
      },
    ];
  } else {
    cols = [
      {
        name: 'tone',
        label: 'TONE',
        cell: (cell) => (
          <DetermineToneIcon
            tone={cell.original.tonePools ? cell.original.tonePools[0]?.value : 0}
          />
        ),
      },
      {
        name: 'headline',
        label: 'HEADLINE',
        cell: (cell) => <div className="headline">{cell.original.headline}</div>,
        width: 5,
      },
    ];
  }
  return cols;
};
