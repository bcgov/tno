import { formatDate } from 'features/utils';
import moment from 'moment';
import React from 'react';
import { FaPlayCircle } from 'react-icons/fa';
import { FaCopyright, FaEyeSlash, FaGripVertical, FaSquareUpRight } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import { Checkbox, Col, IColProps, IContentModel, Row, Show } from 'tno-core';

import { ContentListContext } from './ContentListContext';
import { ContentReportPin } from './ContentReportPin';
import * as styled from './styled';
import { determineToneIcon, truncateTeaser } from './utils';

export interface IContentRowProps extends IColProps {
  selected: IContentModel[];
  item: IContentModel;
  onCheckboxChange: (item: IContentModel, checked: boolean) => void;
  canDrag?: boolean;
  showDate?: boolean;
  popOutIds?: string;
  styleOnSettings?: boolean;
  showSeries?: boolean;
  showTime?: boolean;
}

export const ContentRow: React.FC<IContentRowProps> = ({
  selected,
  item,
  onCheckboxChange,
  styleOnSettings,
  canDrag,
  showDate,
  showSeries,
  showTime,
  popOutIds,
  ...rest
}) => {
  const {
    viewOptions,
    setActiveStream,
    activeFileReference,
    setActiveFileReference,
    activeStream,
    groupBy,
  } = React.useContext(ContentListContext);

  return (
    <styled.ContentRow {...rest}>
      <Row className="parent-row">
        <Show visible={canDrag}>
          <FaGripVertical className="grip" />
        </Show>
        <Checkbox
          className="checkbox"
          checked={selected.some((selectedItem) => selectedItem.id === item.id)}
          onChange={(e) => {
            // TODO
            // e.stopPropagation() does not work, so above we check if the click was inside a checkbox
            onCheckboxChange(item, e.target.checked);
          }}
        />
        {viewOptions.sentiment && determineToneIcon(item.tonePools[0])}
        {showDate && (
          <div className="date">{`${formatDate(item.publishedOn)} ${
            showTime ? `(${moment(item.publishedOn).format('HH:mm')})` : ''
          }`}</div>
        )}
        <Link to={`/view/${item.id}`} className="headline">
          {item.headline}
        </Link>
        <Show visible={groupBy === 'time'}>
          {item.source && <div className="source">{item.source.name}</div>}
        </Show>
        <Show visible={viewOptions.section}>
          {item.section && <div className="section">{item.section}</div>}
          {item.page && <div className="page-number">{item.page}</div>}
        </Show>
        <Show visible={showSeries}>
          {item.series && <div className="series">{item.series.name}</div>}
        </Show>
        <Show visible={styleOnSettings && popOutIds?.includes(String(item.mediaTypeId))}>
          <FaSquareUpRight
            className={`new-tab ${!item.section && 'no-section'}`}
            onClick={(e) => {
              e.stopPropagation();
              window.open(`/view/${item.id}`, '_blank');
            }}
          />
        </Show>
        <Show
          visible={
            !!item.fileReferences.length &&
            !activeStream?.source &&
            (item.fileReferences[0].contentType.includes('video') ||
              item.fileReferences[0].contentType.includes('audio'))
          }
        >
          <FaPlayCircle
            className="play-icon"
            onClick={(e) => {
              e.stopPropagation();
              setActiveFileReference(item.fileReferences[0]);
            }}
          />
        </Show>
        <Show visible={!!activeStream && item.id === activeStream.id}>
          <FaEyeSlash
            className="eye-slash"
            onClick={(e) => {
              e.stopPropagation();
              setActiveStream({ source: '', id: 0 });
              setActiveFileReference(undefined);
            }}
          />
        </Show>
        <ContentReportPin contentId={item.id} />
      </Row>
      <Row>
        {viewOptions.teaser && !!item.body && (
          <div className={`teaser ${canDrag && 'with-grip'}`}>{truncateTeaser(item.body, 250)}</div>
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
