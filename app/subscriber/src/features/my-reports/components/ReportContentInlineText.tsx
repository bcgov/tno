import moment from 'moment';
import { ContentTypeName } from 'tno-core';

import { IReportInstanceContentForm } from '../interfaces';

export interface IReportContentInlineTextProps {
  row: IReportInstanceContentForm;
  userId: number;
}

export const ReportContentInlineText: React.FC<IReportContentInlineTextProps> = ({
  row,
  userId,
}) => {
  if (!row || !row.content) return null;

  let contentDetails = '';

  if (row.content.contentType === ContentTypeName.AudioVideo) {
    // AV Content format
    contentDetails = `
      ${
        row.content.isApproved
          ? `<img src="${process.env.PUBLIC_URL}/assets/transcript_icon.png" alt="Transcript" />`
          : ''
      }
      ${row.content.source?.name ? `${row.content.source.name} | ` : ''}
      ${row.content.series ? `(${row.content.series.name}) | ` : ''}
      ${row.content.contentType ? `${row.content.contentType} | ` : ''}
      ${row.content.publishedOn ? moment(row.content.publishedOn).format('DD-MMM-YYYY') : ''}
      `;
  } else {
    // General Content format
    contentDetails =
      `${
        row.content.versions?.[userId]?.byline
          ? `${row.content.versions[userId].byline ?? ''} | `
          : row.content.byline
          ? `${row.content.byline} | `
          : ''
      }` +
      `${row.content.otherSource ? `${row.content.otherSource} | ` : ''}` +
      `${row.content.page ? `P.${row.content.page} | ` : ''}` +
      `${row.content.publishedOn ? moment(row.content.publishedOn).format('DD-MMM-YYYY') : ''}`;
  }

  return (
    <div
      className="report-content-inline-text"
      dangerouslySetInnerHTML={{ __html: contentDetails }}
    />
    //   {contentDetails}
    // </div>
  );
};
