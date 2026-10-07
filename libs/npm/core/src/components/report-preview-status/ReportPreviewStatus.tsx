import React from 'react';
import { FaCircleCheck, FaTriangleExclamation } from 'react-icons/fa6';

import { AISectionStatusName, IReportAISectionStatusModel } from '../../hooks';
import { Spinner } from '../spinners';
import { isAISectionPending } from './isAISectionPending';
import * as styled from './ReportPreviewStatusStyled';

export interface IReportPreviewStatusProps {
  /** Whether the preview request is running. */
  isRequesting?: boolean;
  /** The AI section states returned with the preview. */
  sections?: IReportAISectionStatusModel[];
  className?: string;
}

/**
 * ReportPreviewStatus shows whether a report preview is being generated, and the state of each of
 * its AI sections while the reporting service generates them.
 * @param param0 Component properties.
 * @returns Component, or nothing when the preview is complete.
 */
export const ReportPreviewStatus: React.FC<IReportPreviewStatusProps> = ({
  isRequesting,
  sections,
  className,
}) => {
  const isPending = isAISectionPending(sections);
  const failed = sections?.filter((s) => s.status === AISectionStatusName.Failed) ?? [];

  if (!isRequesting && !isPending && !failed.length) return null;

  return (
    <styled.ReportPreviewStatus
      className={`report-preview-status${className ? ` ${className}` : ''}`}
      role="status"
      aria-live="polite"
    >
      {isRequesting && (
        <div className="status-row">
          <Spinner size="1rem" />
          <span>Generating the preview…</span>
        </div>
      )}
      {!isRequesting &&
        sections?.map((section) => {
          const label = section.label || section.name;
          switch (section.status) {
            case AISectionStatusName.NotStarted:
              return (
                <div key={section.sectionId} className="status-row">
                  <Spinner size="1rem" />
                  <span>
                    AI section <b>{label}</b>: waiting for the reporting service.
                  </span>
                </div>
              );
            case AISectionStatusName.Generating:
              return (
                <div key={section.sectionId} className="status-row">
                  <Spinner size="1rem" />
                  <span>
                    AI section <b>{label}</b>: generating. The preview updates when it is ready.
                  </span>
                </div>
              );
            case AISectionStatusName.Failed:
              return (
                <div key={section.sectionId} className="status-row status-failed">
                  <FaTriangleExclamation />
                  <span className="status-error">
                    AI section <b>{label}</b> failed
                    {section.error ? `: ${section.error}` : '.'}
                  </span>
                </div>
              );
            default:
              return isPending ? (
                <div key={section.sectionId} className="status-row">
                  <FaCircleCheck />
                  <span>
                    AI section <b>{label}</b>: ready.
                  </span>
                </div>
              ) : null;
          }
        })}
    </styled.ReportPreviewStatus>
  );
};
