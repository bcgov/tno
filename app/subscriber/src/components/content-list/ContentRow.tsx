import { formatSearch } from 'features/search-page/utils';
import React from 'react';
import { FaCopyright, FaEyeSlash, FaX } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import { Checkbox, Col, ContentTypeName, IColProps, IContentModel, Row, Show } from 'tno-core';

import { Attributes } from './Attributes';
import { ContentListContext } from './ContentListContext';
import { ContentReportPin } from './ContentReportPin';
import * as styled from './styled';
import { determineToneIcon, truncateTeaser } from './utils';

export interface IContentRowProps extends IColProps {
  selected: IContentModel[];
  item: IContentModel;
  canDrag?: boolean;
  showDate?: boolean;
  popOutIds?: string;
  showSeries?: boolean;
  showTime?: boolean;
  filter?: any;
  onCheckboxChange: (item: IContentModel, checked: boolean) => void;
  onRemove?: (item: IContentModel) => void;
}

export const ContentRow: React.FC<IContentRowProps> = ({
  selected,
  item,
  canDrag,
  showDate,
  showSeries,
  showTime,
  popOutIds,
  filter,
  onCheckboxChange,
  onRemove,
  ...rest
}) => {
  const {
    viewOptions,
    setActiveStream,
    activeFileReference,
    setActiveFileReference,
    activeStream,
  } = React.useContext(ContentListContext);

  const body = React.useMemo(() => {
    const truncated = truncateTeaser(item.body, 250);
    return formatSearch(truncated, filter);
  }, [filter, item.body]);
  const headline = React.useMemo(() => {
    return formatSearch(item.headline, filter);
  }, [filter, item.headline]);

  return (
    <styled.ContentRow {...rest}>
      <Row className="parent-row">
        <Checkbox
          className="checkbox"
          checked={selected.some((selectedItem) => selectedItem.id === item.id)}
          onChange={(e) => {
            // TODO
            // e.stopPropagation() does not work, so above we check if the click was inside a checkbox
            onCheckboxChange(item, e.target.checked);
          }}
        />
        <Show visible={canDrag}>
          <img src={`${process.env.PUBLIC_URL}/assets/elipsis.svg`} alt="Drag" className="grip" />
        </Show>
        {viewOptions.sentiment && determineToneIcon(item.tonePools[0])}
        {item.contentType === ContentTypeName.AudioVideo && !!item.body && (
          <img
            className="transcript-feather"
            src={`${process.env.PUBLIC_URL}/assets/transcript_feather.svg`}
            alt="Transcript"
          />
        )}
        <Link to={`/view/${item.id}`} className="headline">
          <div>{headline}</div>
        </Link>
        <Attributes
          item={item}
          showDate={showDate}
          showTime={showTime}
          showSeries={showSeries}
          viewOptions={viewOptions}
        />
        <Row className="icon-row" nowrap>
          {popOutIds?.includes(String(item.mediaTypeId)) ? (
            <img
              src={`${process.env.PUBLIC_URL}/assets/mediaplay-newwindow.svg`}
              className={`icon new-tab ${!item.section && 'no-section'}`}
              alt="Pop-out to play"
              data-tooltip-id="main-tooltip"
              data-tooltip-content="Pop-out to play"
              onClick={(e) => {
                e.stopPropagation();
                window.open(
                  `/view/${item.id}`,
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
              src={`${process.env.PUBLIC_URL}/assets/mediaplay-inline.svg`}
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
      <Attributes
        mobile
        item={item}
        showDate={showDate}
        showTime={showTime}
        showSeries={showSeries}
        viewOptions={viewOptions}
      />
      <Row>
        {viewOptions.teaser && !!item.body && (
          <div className={`teaser ${canDrag && 'with-grip'}`}>{body}</div>
        )}
        <Show visible={!!activeStream?.source && activeStream.id === item.id}>
          <Col className="media-playback">
            {activeFileReference?.contentType.includes('audio') && (
              <audio controls src={activeStream?.source} />
            )}
            {activeFileReference?.contentType.includes('video') && (
              <video controls src={activeStream?.source} />
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
