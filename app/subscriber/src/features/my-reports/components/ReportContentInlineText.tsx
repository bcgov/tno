import moment from 'moment';
import { ContentTypeName } from 'tno-core';

import { IReportInstanceContentForm } from '../interfaces';

export interface IReportContentInlineTextProps {
  row: IReportInstanceContentForm;
}

export const ReportContentInlineText: React.FC<IReportContentInlineTextProps> = ({ row }) => {
  if (!row || !row.content) return null;

  let contentDetails = '';

  if (row.content.contentType === ContentTypeName.AudioVideo) {
    // AV Content format
    contentDetails =
      `${row.content.source?.name ? `${row.content.source.name} | ` : ''}` +
      `${row.content.series ? `(${row.content.series.name}) | ` : ''}` +
      `${row.content.contentType ? `${row.content.contentType} | ` : ''}` +
      `${row.content.publishedOn ? moment(row.content.publishedOn).format('DD-MMM-YYYY') : ''}`;
  } else {
    // General Content format
    contentDetails =
      `${row.content.byline ? `${row.content.byline} | ` : ''}` +
      `${row.content.otherSource ? `${row.content.otherSource} | ` : ''}` +
      `${row.content.page ? `P.${row.content.page} | ` : ''}` +
      `${row.content.publishedOn ? moment(row.content.publishedOn).format('DD-MMM-YYYY') : ''}`;
  }

  return <div className="report-content-inline-text">{contentDetails}</div>;
};
