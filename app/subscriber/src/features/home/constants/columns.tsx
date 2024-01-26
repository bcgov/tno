import { Sentiment } from 'components/sentiment';
import { IContentSearchResult } from 'features/utils/interfaces';
import { FaCopyright, FaFeather, FaPlayCircle } from 'react-icons/fa';
import { ContentTypeName, ITableHookColumn, Show } from 'tno-core';

export const determineColumns = (
  contentType: ContentTypeName | 'all',
  windowWidth?: number,
  hide?: string[],
  media?: Function,
) => {
  const formatDate = (date: string) => {
    const d = new Date(date);
    const day = d.toLocaleDateString('en-us', { day: '2-digit' });
    const month = d.toLocaleDateString('en-us', { month: 'short' }).toUpperCase();
    const year = d.toLocaleDateString('en-us', { year: 'numeric' });
    return `${day}-${month}-${year}`;
  };

  const removeHtmlTags = (html: string | undefined) => {
    if (html === undefined) return '';
    return html.replace(/(<([^>]+)>)/gi, '');
  };

  const extractTeaser = (text: string | undefined) => {
    if (text === undefined || text === '') return false;
    const fullText = removeHtmlTags(text);
    return `${fullText.substring(0, 220)} ...`;
  };

  const playMedia = (p: IContentSearchResult) => {
    !!media && media(p);
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
                <div className="headline">
                  {cell.original.headline}
                  <Show visible={cell.original.hasTranscript}>
                    <FaFeather className="fa-lg trancript-icon" title="Has Transcript" />
                  </Show>
                </div>
              </td>
            </tr>
            {!hide?.includes('teaser') && cell.original.body !== '' && (
              <tr>
                <td colSpan={2}>
                  <div className="teaser">{extractTeaser(cell.original.body)}</div>
                </td>
              </tr>
            )}
            {!hide?.includes('teaser') &&
              cell.original.contentType === ContentTypeName.AudioVideo && (
                <tr>
                  <td colSpan={2} align="center">
                    <Show visible={!!cell.original.displayMedia && cell.original.displayMedia}>
                      <Show
                        visible={
                          cell.original.fileReferences.length > 0 &&
                          cell.original.fileReferences[0].contentType.startsWith('audio/') &&
                          !!cell.original.displayMedia &&
                          cell.original.displayMedia
                        }
                      >
                        <audio controls>
                          <source
                            src={cell.original.mediaUrl}
                            type={
                              cell.original.fileReferences.length > 0
                                ? cell.original.fileReferences[0].contentType
                                : ''
                            }
                          />
                          HTML5 Audio is required
                        </audio>
                      </Show>
                      <Show
                        visible={
                          cell.original.fileReferences.length > 0 &&
                          cell.original.fileReferences[0].contentType.startsWith('video/') &&
                          !!cell.original.displayMedia &&
                          cell.original.displayMedia
                        }
                      >
                        <video
                          controls
                          height={windowWidth! > 500 ? 389 : 135}
                          width={windowWidth! > 500 ? 488 : 240}
                          preload="metadata"
                        >
                          <source
                            src={cell.original.mediaUrl}
                            type={
                              cell.original.fileReferences.length > 0
                                ? cell.original.fileReferences[0].contentType
                                : ''
                            }
                          />
                          HTML5 Audio is required
                        </video>
                      </Show>
                      <div className="copyrightParent">
                        <div className="copyrightIcon">
                          <FaCopyright />
                        </div>
                        <div className="copyrightText">
                          Copyright protected and owned by broadcaster. Your licence is limited to
                          internal, non-commercial, government use. All reproduction, broadcast,
                          transmission, or other use of this work is prohibited and subject to
                          licence.
                        </div>
                      </div>
                    </Show>
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
  const printCols: ITableHookColumn<IContentSearchResult>[] = [
    {
      accessor: 'sectionPage',
      label: '',
      isVisible: !hide?.includes('sectionPage'),
      cell: (cell) => (
        <>
          <Show visible={cell.original.contentType === ContentTypeName.AudioVideo}>
            <FaPlayCircle
              className="fa-lg"
              onClick={(e) => {
                e.stopPropagation();
                playMedia(cell.original);
              }}
            />
          </Show>
          <Show visible={cell.original.contentType !== ContentTypeName.AudioVideo}>
            <div className="section">{`${cell.original.section}: ${cell?.original.page}`}</div>
          </Show>
        </>
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
