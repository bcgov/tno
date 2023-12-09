import React from 'react';
import { FaArrowLeft, FaEnvelope } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { Checkbox, IContentModel, Row, Show } from 'tno-core';

import { AddToFolderMenu } from './AddToFolderMenu';
import { AddToReportMenu } from './AddToReportMenu';
import * as styled from './styled';

export interface IContentActionBarProps {
  content: IContentModel[];
  className?: string;
  /** whether or not this is being used on a list view */
  onList?: boolean;
}
export const ContentActionBar: React.FC<IContentActionBarProps> = ({
  className,
  content,
  onList,
}) => {
  const navigate = useNavigate();
  return (
    <styled.ContentActionBar className={`${className} ${onList ? 'list-view' : ''}`}>
      <Show visible={!onList}>
        <div className="action left-side-items" onClick={() => navigate(-1)}>
          <FaArrowLeft className="back-arrow" />
          BACK TO HEADLINES
        </div>
      </Show>
      <Show visible={onList}>
        <Row className="select-all">
          <div className="check-area">
            <Checkbox />
            SELECT ALL
          </div>
        </Row>
        <div className="arrow" />
      </Show>
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
