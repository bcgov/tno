import { Sentiment } from 'components/sentiment';
import { ContentTypeName, IContentModel, ITableHookColumn } from 'tno-core';

export const determineColumns = (
  contentType: ContentTypeName | 'all',
  windowWidth?: number,
  hide?: string[],
) => {
  // columns common to all content
  const baseCols: ITableHookColumn<IContentModel>[] = [
    {
      accessor: 'tone',
      label: 'TONE',
      // width: 0.25,
      isVisible: !hide?.includes('tone'),
      cell: (cell) => (
        <Sentiment value={cell.original.tonePools ? cell.original.tonePools[0]?.value : 0} />
      ),
    },
    {
      accessor: 'headline',
      label: 'HEADLINE',
      cell: (cell) => <div className="headline">{cell.original.headline}</div>,
      width: windowWidth && windowWidth > 1000 ? 4 : 0.85,
    },
  ];
  // columns specific to print content
  const printCols: ITableHookColumn<IContentModel>[] = [
    {
      accessor: 'sectionPage',
      label: 'SECTION PAGE',
      isVisible: !hide?.includes('sectionPage'),
      cell: (cell) => (
        <div className="section">{`${cell.original.section}: ${cell?.original.page}`}</div>
      ),
      width: windowWidth && windowWidth > 1000 ? 1 : 0.5,
    },
  ];
  if (contentType === ContentTypeName.PrintContent || contentType === 'all') {
    return [...baseCols, ...printCols];
  } else {
    return baseCols;
  }
};
