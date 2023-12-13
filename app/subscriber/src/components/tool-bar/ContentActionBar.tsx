import React from 'react';
import { FaArrowLeft, FaEnvelope } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { Checkbox, IContentModel, Row, Show } from 'tno-core';

import { AddToFolderMenu } from './AddToFolderMenu';
import { AddToReportMenu } from './AddToReportMenu';
import * as styled from './styled';
import { selectAllCheckboxes } from './utils';

export interface IContentActionBarProps {
  content: IContentModel[];
  className?: string;
  /** whether or not this is being used on a list view */
  onList?: boolean;
  /** the zone to select all checkboxes in */
  selectAllZone?: Element;
  setSelectAll?: (value: boolean) => void;
}
export const ContentActionBar: React.FC<IContentActionBarProps> = ({
  className,
  content,
  selectAllZone,
  onList,
  setSelectAll,
}) => {
  const navigate = useNavigate();
  let handleSelectAll;

  // in order to ensure no unwanted checkboxes are selected, we must pass the zone in which the checkboxes are located
  if (!!selectAllZone) {
    const checkboxes = selectAllZone.querySelectorAll(
      'input[type="checkbox"]',
    ) as NodeListOf<HTMLInputElement>;

    handleSelectAll = (e: React.ChangeEvent<HTMLInputElement>) => {
      setSelectAll?.(e.target.checked);
      selectAllCheckboxes(checkboxes, e.target.checked);
    };
  }

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
            {!!handleSelectAll && (
              <Row>
                <Checkbox onChange={handleSelectAll} />
                SELECT ALL
              </Row>
            )}
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
