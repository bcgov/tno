import moment from 'moment';
import { ContentTypeName } from 'tno-core';

import { IReportInstanceContentForm } from '../interfaces';

export const reportInstanceContentInlineDisplay = (row: IReportInstanceContentForm) => {
  if (!row.content) return '';
  if (row.content.contentType === ContentTypeName.AudioVideo) {
    // AV Content format
    console.log('xxx1', row.content.series);
    return (
      `${row.content.source?.name ? `${row.content.source.name} | ` : ''}` +
      `${row.content.series ? `(${row.content.series.name}) | ` : ''}` +
      `${row.content.contentType ? `${row.content.contentType} | ` : ''}` +
      `${row.content.publishedOn ? moment(row.content.publishedOn).format('DD-MMM-YYYY') : ''}`
    );
  } else {
    // General Content format
    return (
      `${row.content.byline ? `${row.content.byline} | ` : ''}` +
      `${row.content.otherSource ? `${row.content.otherSource} | ` : ''}` +
      `${row.content.page ? `P.${row.content.page} | ` : ''}` +
      `${row.content.publishedOn ? moment(row.content.publishedOn).format('DD-MMM-YYYY') : ''}`
    );
  }
};
