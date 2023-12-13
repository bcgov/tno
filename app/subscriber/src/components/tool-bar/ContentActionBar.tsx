import { ShareMenu } from 'components/share-menu';
import React from 'react';
import { FaArrowLeft } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { Checkbox, IContentModel, Row, Show } from 'tno-core';

import { AddToFolderMenu } from './AddToFolderMenu';
import { AddToReportMenu } from './AddToReportMenu';
import * as styled from './styled';

export interface IContentActionBarProps {
  /** An array of content */
  content: IContentModel[];
  /** Class name */
  className?: string;
  /** Whether the back button is displayed */
  showBackButton?: boolean;
  /** Event fires when back button is pressed */
  onBack?: () => void;
  /** Event fires when select all checkbox is changed */
  onSelectAll?: React.ChangeEventHandler<HTMLInputElement>;
}
export const ContentActionBar: React.FC<IContentActionBarProps> = ({
  className,
  content,
  showBackButton,
  onBack,
  onSelectAll,
}) => {
  const navigate = useNavigate();

  return (
    <styled.ContentActionBar className={className}>
      <Show visible={showBackButton}>
        <div className="action left-side-items" onClick={() => (onBack ? onBack() : navigate(-1))}>
          <FaArrowLeft className="back-arrow" />
          BACK TO HEADLINES
        </div>
      </Show>
      <Show visible={!!onSelectAll}>
        <Row className="select-all">
          <div className="check-area">
            <Row gap="0.25rem">
              <Checkbox id="select-all" onChange={onSelectAll} />
              <label htmlFor="select-all">SELECT ALL</label>
            </Row>
          </div>
        </Row>
        <div className="arrow" />
      </Show>
      <div className="right-side-items">
        <Row>
          <ShareMenu content={content} />
          <AddToFolderMenu content={content} />
          <AddToReportMenu content={content} />
        </Row>
      </div>
    </styled.ContentActionBar>
  );
};
