import React from 'react';
import { FaArrowLeft, FaEnvelope } from 'react-icons/fa6';
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
      <div className="action left-side-items" onClick={() => navigate(-1)}>
        <FaArrowLeft className="back-arrow" />
        BACK TO HEADLINES
      </div>
      <div className="right-side-items">
        <Row>
          <div className="action">
            <FaEnvelope /> <span>SHARE</span>
          </div>
          <AddToFolderMenu content={content} />
          <AddToReportMenu content={content} />
        </Row>
      </div>
    </styled.ContentActionBar>
  );
};
