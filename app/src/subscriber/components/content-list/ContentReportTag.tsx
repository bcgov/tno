import React from 'react';
import { FaCircle } from 'react-icons/fa';
import { useProfileStore } from 'store/slices';
import { IReportModel } from 'tno-core';

import * as styled from './styled';

export interface IContentReportTagProps {
  /** The content ID */
  contentId: number;
}

/**
 * Displays a list of tags with report name for specified 'contentId'.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentReportTag = ({ contentId }: IContentReportTagProps) => {
  const [{ reportContent, myReports }] = useProfileStore();
  const [reports, setReports] = React.useState<IReportModel[]>([]);
  const tagColors = [
    'red',
    'yellow',
    'blue300',
    'lime',
    'violet',
    'emerald',
    'blue700',
    'orange',
    'indigo',
    'fuschia ',
  ];

  React.useEffect(() => {
    setReports(
      Object.keys(reportContent)
        .filter((reportId) => {
          const content = reportContent[+reportId];
          return content.some((c) => c === contentId);
        })
        .map((reportId) => myReports.find((r) => r.id === +reportId)!)
        .filter((report) => report !== undefined),
    );
  }, [contentId, myReports, reportContent]);

  if (!reports.length) return <div></div>;

  return (
    <styled.ContentReportTag>
      <div className="tag-container">
        {reports.map((item, index) => (
          <div key={index} className="report-tag">
            <FaCircle className={`tag-icon ${tagColors[index % 10].toString()}`} />
            <span className="report-tag-text">{item.name}</span>
          </div>
        ))}
      </div>
    </styled.ContentReportTag>
  );
};
