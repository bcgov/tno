import { ShareMenu } from 'components/share-menu';
import React, { useState } from 'react';
import { FaArrowUpRightFromSquare } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { useLookup, useSettings } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Checkbox, IContentModel, Row, Settings, Show } from 'tno-core';

import { AddToFolderMenu } from './AddToFolderMenu';
import { AddToReportMenu } from './AddToReportMenu';
import { RemoveFromFolder } from './RemoveFromFolder';
import * as styled from './styled';

export interface IContentActionBarProps {
  /** An array of content */
  content: IContentModel[];
  /** Class name */
  className?: string;
  /** Whether the user is viewing a single piece of content, helps with logic for conditional rendering */
  viewingContent?: boolean;
  /** Event fires when back button is pressed */
  onBack?: () => void;
  /** Event fires when select all checkbox is changed */
  onSelectAll?: React.ChangeEventHandler<HTMLInputElement>;
  /** Whether to show remove from folder button */
  removeFolderItem?: Function;
  /** whether to disable AddtoFolderMenu */
  disableAddToFolder?: boolean;
  /** Event to fire when selection should be cleared */
  onClear?: () => void;
}

export const ContentActionBar: React.FC<IContentActionBarProps> = ({
  className,
  content,
  viewingContent,
  onBack,
  onSelectAll,
  onClear,
  removeFolderItem,
  disableAddToFolder,
}) => {
  const navigate = useNavigate();
  const [{ frontPageImagesMediaTypeId, settings, isReady }] = useLookup();
  const { editorUrl } = useSettings();
  const [{ userInfo }] = useAppStore();
  const [isFrontPageImage, setIsFrontPageImage] = useState(true);
  const [contentId, setContentId] = useState(content?.[0]?.id ?? null);
  // Only show action items in the bar if it's NOT front page image content
  const showActionsItems = !isFrontPageImage;

  React.useEffect(() => {
    // Every time content changes, assume it's a front page image until we know it's not
    // this prevents rendering the action items until we know they're needed
    if (contentId !== content?.[0]?.id) {
      setIsFrontPageImage(true);
      setContentId(content?.[0]?.id);
    }
  }, [content, contentId]);

  React.useEffect(() => {
    if (isReady) {
      const typeId =
        frontPageImagesMediaTypeId ??
        settings.find((s) => s.name === Settings.FrontPageImageMediaType)?.value;
      if (typeId && +typeId !== content?.[0]?.mediaTypeId) {
        setIsFrontPageImage(false);
      }
    }
  }, [frontPageImagesMediaTypeId, settings, isReady, content]);

  return (
    <styled.ContentActionBar className={className}>
      <Show visible={viewingContent && userInfo?.roles.includes('editor')}>
        <div className="action left-side-items" onClick={() => (onBack ? onBack() : navigate(-1))}>
          <img
            className="back-button"
            src={'/assets/back-button.svg'}
            alt="Back"
            data-tooltip-id="back-button"
          />
          <Tooltip
            clickable={false}
            className="back-tooltip"
            classNameArrow="back-tooltip-arrow"
            id="back-button"
            place="top"
            noArrow={false}
            content="Back"
          />
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
      {showActionsItems && (
        <div className="right-side-items">
          <Row>
            <ShareMenu content={content} />
            {disableAddToFolder ? null : <AddToFolderMenu onClear={onClear} content={content} />}
            <AddToReportMenu content={content} onClear={onClear} />
            {!!removeFolderItem && <RemoveFromFolder onClick={removeFolderItem} />}
            {viewingContent && (
              <button
                className="editor-button"
                onClick={() => {
                  if (editorUrl) {
                    window.open(`${editorUrl}/contents/${contentId}`, '_blank');
                  } else {
                    toast.error('Editor URL not found, please set and try again.');
                  }
                }}
              >
                <FaArrowUpRightFromSquare />
                Edit Story
              </button>
            )}
          </Row>
        </div>
      )}
    </styled.ContentActionBar>
  );
};
