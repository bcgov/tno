import { Sentiment } from 'components/sentiment';
import { IContentSearchResult } from 'features/utils/interfaces';
import { ContentTypeName, ITableHookColumn, Show } from 'tno-core';

export const determineColumns = (contentType: ContentTypeName | 'all', hide?: string[]) => {
  const formatDate = (date: string) => {
    const d = new Date(date);
    const day = d.toLocaleDateString('en-us', { day: '2-digit' });
    const month = d.toLocaleDateString('en-us', { month: 'short' }).toUpperCase();
    const year = d.toLocaleDateString('en-us', { year: 'numeric' });
    return `${day}-${month}-${year}`;
  };

  // columns common to all content
  const baseCols: ITableHookColumn<IContentSearchResult>[] = [
    {
      accessor: 'tone',
      label: '',
      isVisible: !hide?.includes('tone'),
      cell: (cell) => (
        <Sentiment value={cell.original.tonePools ? cell.original.tonePools[0]?.value : 0} />
      ),
    },
    {
      accessor: 'headline',
      label: '',
      cell: (cell) => (
        <table className="tableHeadline">
          <tbody>
            <tr>
              <td className="dateColumn td-date">
                <div className="date">{formatDate(cell.original.publishedOn)}</div>
              </td>
              <td className="headlineColumn">
                <div className="headline">{cell.original.headline}</div>
              </td>
            </tr>
          </tbody>
        </table>
      ),
    },
  ];
  // columns specific to print content
  const printCols: ITableHookColumn<IContentSearchResult>[] = [
    {
      accessor: 'sectionPage',
      label: '',
      isVisible: !hide?.includes('sectionPage'),
      cell: (cell) => (
        <>
          <Show visible={cell.original.contentType !== ContentTypeName.AudioVideo}>
            <div className="section">{`${cell.original.section}: ${cell?.original.page}`}</div>
          </Show>
        </>
      ),
    },
  ];

  // columns specific to my minister mentions
  const mentionCols: ITableHookColumn<IContentSearchResult>[] = [
    {
      accessor: 'mentions',
      label: '',
      isVisible: !hide?.includes('mentions'),
      cell: (cell) => (
        <div className="mentionsColumn">
          <div className="mentions">
            {cell.original.ministerMentions?.map((m) => {
              return (
                <div key={`${cell.original.id}-${m}`} className="mentionTag">
                  {m}
                </div>
              );
            })}
          </div>
        </div>
      ),
    },
  ];

  if (contentType === ContentTypeName.PrintContent || contentType === 'all') {
    return [...baseCols, ...printCols, ...mentionCols];
  } else {
    return baseCols;
  }
};
