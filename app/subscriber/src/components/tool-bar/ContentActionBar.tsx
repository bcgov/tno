import React from 'react';
import { FaArrowLeft, FaEnvelope, FaFileExport } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { IContentModel, Row } from 'tno-core';

import { AddToFolderMenu } from './AddToFolderMenu';
import { AddToReportMenu } from './AddToReportMenu';
import * as styled from './styled';

export interface IContentActionBarProps {
  content: IContentModel[];
  className: string;
}
export const ContentActionBar: React.FC<IContentActionBarProps> = ({ className, content }) => {
  const navigate = useNavigate();
  return (
    <styled.ContentActionBar className={className}>
      <div className="left-side-items">
        <FaArrowLeft onClick={() => navigate(-1)} />
        BACK TO HEADLINES
      </div>
      <div className="right-side-items">
        <Row>
          <div className="action">
            <FaEnvelope /> SHARE
          </div>
          <AddToFolderMenu content={content} />
          <AddToReportMenu content={content} />
        </Row>
      </div>
    </styled.ContentActionBar>
  );
};
