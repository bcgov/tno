import { Sentiment } from 'components/sentiment';
import { ContentTypeName, IContentModel, ITableHookColumn } from 'tno-core';

export const determineColumns = (
  contentType: ContentTypeName | 'all',
  windowWidth?: number,
  hide?: string[],
) => {
  const formatDate = (date: string) => {
    const d = new Date(date);
    const day = d.toLocaleDateString('en-us', { day: '2-digit' });
    const month = d.toLocaleDateString('en-us', { month: 'short' }).toUpperCase();
    const year = d.toLocaleDateString('en-us', { year: 'numeric' });
    return `${day}-${month}-${year}`;
  };

  const removeHtmlTags = (html: string | undefined) => {
    if (html === undefined || html === '') return false;
    return html.replace(/(<([^>]+)>)/gi, '');
  };
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
      cell: (cell) => (
        <table className="tableHeadline">
          <tbody>
            <tr>
              <td className="dateColumn">
                <div className="date">{formatDate(cell.original.publishedOn)}</div>
              </td>
              <td className="headlineColumn">
                <div className="headline">{cell.original.headline}</div>
              </td>
            </tr>
            {!hide?.includes('teaser') && (
              <tr>
                <td colSpan={2}>
                  <div className="teaser">{removeHtmlTags(cell.original.body)}</div>
                </td>
              </tr>
            )}
          </tbody>
        </table>
      ),
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
