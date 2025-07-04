import { formatSearch } from 'features/search-page/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import React from 'react';
import { FaCopyright, FaEyeSlash, FaX } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import {
  Checkbox,
  Col,
  ContentTypeName,
  IColProps,
  IContentModel,
  IFileReferenceModel,
  IFilterSettingsModel,
  Row,
  Show,
} from 'tno-core';

import { Attributes } from './Attributes';
import { ContentListContext } from './ContentListContext';
import { ContentReportPin } from './ContentReportPin';
import * as styled from './styled';
import { determineToneIcon, truncateTeaser } from './utils';
import { highlightTerms } from './utils/highlightTerms';

export interface IContentRowProps extends IColProps {
  selected: IContentModel[];
  item: IContentSearchResult;
  highlighTerms?: string[];
  canDrag?: boolean;
  showDate?: boolean;
  popOutIds?: string;
  showSeries?: boolean;
  showTime?: boolean;
  filter?: IFilterSettingsModel;
  onCheckboxChange: (item: IContentModel, checked: boolean) => void;
  onRemove?: (item: IContentModel) => void;
  activeStream: { id: number; source: string };
  setActiveStream: (stream: { id: number; source: string }) => void;
  activeFileReference: IFileReferenceModel | undefined;
  setActiveFileReference: (fileReference: IFileReferenceModel | undefined) => void;
  simpleView?: boolean;
}

export const ContentRow: React.FC<IContentRowProps> = ({
  selected,
  item,
  highlighTerms,
  canDrag,
  showDate,
  showSeries,
  showTime,
  popOutIds,
  filter = { search: '', size: 100, searchUnpublished: false },
  onCheckboxChange,
  onRemove,
  simpleView,
  activeStream,
  setActiveStream,
  activeFileReference,
  setActiveFileReference,
  ...rest
}) => {
  const { viewOptions } = React.useContext(ContentListContext);

  const videoRef = React.useRef<HTMLVideoElement>(null);
  const audioRef = React.useRef<HTMLAudioElement>(null);

  const body = React.useMemo(() => {
    const truncated = truncateTeaser(item.body || item.summary, 250);
    return filter.inStory ? formatSearch(truncated, filter) : truncated;
  }, [filter, item.body, item.summary]);
  const headline = React.useMemo(() => {
    return filter.inHeadline ? formatSearch(item.headline, filter) : item.headline;
  }, [filter, item.headline]);

  const bodyTermHighlighted = highlighTerms
    ? highlightTerms(body as string, highlighTerms ?? [])
    : [];
  const headerTermHighlighted = highlighTerms
    ? highlightTerms(headline as string, highlighTerms ?? [])
    : [];

  /** This is a workaround for the autoplay attribute on <video>. Most chromium browser do not allow autoplay with the muted attribute included; however, we can avoid this issue by programatically calling play. The logic below will play the video
   * once the user clicks the inline play button. */
  React.useEffect(() => {
    const videoElement = videoRef.current;
    const audioElement = audioRef.current;
    if (!!videoElement && activeStream?.source) {
      const handleCanPlay = () => {
        videoElement.play().catch((e) => console.error('Failed to play video', e));
      };

      // need to have an event listener to trigger the play, otherwise it will error out on the play call (calls too early)
      videoElement.addEventListener('canplay', handleCanPlay);

      return () => {
        videoElement.removeEventListener('canplay', handleCanPlay);
      };
    }
    if (!!audioElement && activeStream?.source) {
      const handleCanPlay = () => {
        audioElement.play().catch((e) => console.error('Failed to play audio', e));
      };

      audioElement.addEventListener('canplay', handleCanPlay);

      return () => {
        audioElement.removeEventListener('canplay', handleCanPlay);
      };
    }
  }, [activeStream]);

  return (
    <styled.ContentRow simpleView={simpleView} {...rest}>
      <Row className="parent-row">
        <Show visible={!simpleView}>
          <Checkbox
            className="checkbox"
            checked={selected.some((selectedItem) => selectedItem.id === item.id)}
            onChange={(e) => {
              // TODO
              // e.stopPropagation() does not work, so above we check if the click was inside a checkbox
              onCheckboxChange(item, e.target.checked);
            }}
          />
        </Show>
        <Show visible={canDrag && !simpleView}>
          <img src={`/assets/elipsis.svg`} alt="Drag" className="grip" />
        </Show>
        {viewOptions.sentiment && determineToneIcon(item.tonePools[0])}
        {item.contentType === ContentTypeName.AudioVideo && !!item.body && (
          <img
            className="transcript-feather"
            src={`/assets/transcript_feather.svg`}
            alt="Transcript"
          />
        )}
        <Link
          to={`/view${!!item.ministerName ? '/my-minister' : ''}/${item.id}`}
          className="headline"
        >
          <>
            {headerTermHighlighted.length > 0 ? (
              headerTermHighlighted.map((part, index) => (
                <React.Fragment key={index}>{part}</React.Fragment>
              ))
            ) : (
              <div>{headline}</div>
            )}
          </>
        </Link>
        <Show visible={!simpleView}>
          <Attributes
            item={item}
            highlighTerms={highlighTerms ?? []}
            showDate={showDate}
            showTime={showTime}
            showSeries={showSeries}
            viewOptions={viewOptions}
          />
        </Show>
        <Row className="icon-row" nowrap>
          {popOutIds?.includes(String(item.mediaTypeId)) ? (
            <img
              src={`/assets/mediaplay-newwindow.svg`}
              className={`icon new-tab ${!item.section && 'no-section'}`}
              alt="Pop-out to play"
              data-tooltip-id="main-tooltip"
              data-tooltip-content="Pop-out to play"
              onClick={(e) => {
                e.stopPropagation();
                window.open(
                  `/view/${item.id}/popout`,
                  '_blank',
                  'toolbar=yes,scrollbars=yes,resizable=yes,top=500,left=600,width=600,height=465',
                );
              }}
            />
          ) : (
            <div className="popout-placeholder" />
          )}
          {!!item.fileReferences.length &&
          !activeStream?.source &&
          (item.fileReferences[0].contentType.includes('video') ||
            item.fileReferences[0].contentType.includes('audio')) ? (
            <img
              className="play-icon icon"
              data-tooltip-id="main-tooltip"
              data-tooltip-content="Play inline"
              alt="Play inline"
              src={`/assets/mediaplay-inline.svg`}
              onClick={(e) => {
                e.stopPropagation();
                setActiveFileReference(item.fileReferences[0]);
              }}
            />
          ) : (
            <div className="icon-placeholder" />
          )}
          {onRemove && (
            <FaX
              className="icon-remove"
              title="Remove"
              onClick={(e) => {
                e.stopPropagation();
                onRemove(item);
              }}
            />
          )}
          <Show visible={!!activeStream && item.id === activeStream.id}>
            <FaEyeSlash
              className="eye-slash icon"
              onClick={(e) => {
                e.stopPropagation();
                setActiveStream({ source: '', id: 0 });
                setActiveFileReference(undefined);
              }}
            />
          </Show>
          <ContentReportPin contentId={item.id} />
        </Row>
      </Row>
      <Show visible={!simpleView}>
        <Attributes
          mobile
          item={item}
          highlighTerms={highlighTerms ?? []}
          showDate={showDate}
          showTime={showTime}
          showSeries={showSeries}
          viewOptions={viewOptions}
        />
      </Show>
      <Row>
        <Show visible={!simpleView}>
          {viewOptions.teaser && (!!item.body || !!item.summary) && (
            <div className="teaser">
              {bodyTermHighlighted.length > 0 ? (
                bodyTermHighlighted.map((part, index) => (
                  <React.Fragment key={index}>{part}</React.Fragment>
                ))
              ) : (
                <div className="teaser-content">{body}</div>
              )}
            </div>
          )}
        </Show>
        <Show visible={!!activeStream?.source && activeStream.id === item.id}>
          <Col className="media-playback">
            {activeFileReference?.contentType.includes('audio') && (
              <audio controls src={activeStream?.source} ref={audioRef} />
            )}
            {activeFileReference?.contentType.includes('video') && (
              <video controls src={activeStream?.source} ref={videoRef} />
            )}
            <div className="copyright-text">
              <FaCopyright />
              Copyright protected and owned by broadcaster. Your licence is limited to internal,
              non-commercial, government use. All reproduction, broadcast, transmission, or other
              use of this work is prohibited and subject to licence.
            </div>
          </Col>
        </Show>
      </Row>
    </styled.ContentRow>
  );
};
