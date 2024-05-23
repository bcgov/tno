import { formatDate } from 'features/utils';
import moment from 'moment';
import React from 'react';
import { FaPlayCircle } from 'react-icons/fa';
import { FaCopyright, FaEyeSlash, FaGripVertical, FaSquareUpRight } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import { useLookup } from 'store/hooks';
import { Checkbox, Col, ContentTypeName, IColProps, IContentModel, Row, Show } from 'tno-core';

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
  showSeries?: boolean;
  showTime?: boolean;
}

export const ContentRow: React.FC<IContentRowProps> = ({
  selected,
  item,
  onCheckboxChange,
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
  const [{ mediaTypes }] = useLookup();
  console.log(mediaTypes);

  const noByLine = ['Talk Radio', 'News Radio', 'Events'];
  const noSource = ['Events', 'Talk Radio'];
  console.log(item);

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
        <Link to={`/view/${item.id}`} className="headline">
          {item.headline}
        </Link>
        {showDate && <div className="date has-divider">{formatDate(item.publishedOn)}</div>}
        {showTime ||
          (item.contentType !== ContentTypeName.PrintContent && (
            <div className="time has-divider">{`${moment(item.publishedOn).format('HH:mm')}`}</div>
          ))}
        {item.source &&
          !noSource.some((mt) => {
            const mediaTypeObj = mediaTypes.find((m) => m.name === mt);
            return item.mediaTypeId === mediaTypeObj?.id;
          }) && (
            <div className={`source ${item.byline && `has-divider`}`}>{`${item.source.name}`}</div>
          )}
        {/* do not include byline in talk radio, news radio, or events even if present */}
        {item.byline &&
          !noByLine.some((mt) => {
            const mediaTypeObj = mediaTypes.find((m) => m.name === mt);
            return item.mediaTypeId === mediaTypeObj?.id;
          }) && (
            <div className={`byline ${!!item.section && 'has-divider'}`}>{`${item.byline}`}</div>
          )}
        {item.series &&
          noSource.some((mt) => {
            const mediaTypeObj = mediaTypes.find((m) => m.name === mt);
            return item.mediaTypeId === mediaTypeObj?.id;
          }) && <div className="series">{item.series.name}</div>}
        <Show visible={viewOptions.section}>
          {item.section && <div className="section">{item.section}</div>}
          {item.page && <div className="page-number">{item.page}</div>}
        </Show>
        <Show visible={popOutIds?.includes(String(item.mediaTypeId))}>
          <FaSquareUpRight
            className={`new-tab ${!item.section && 'no-section'}`}
            onClick={(e) => {
              e.stopPropagation();
              window.open(
                `/view/${item.id}`,
                '_blank',
                'toolbar=yes,scrollbars=yes,resizable=yes,top=500,left=600,width=600,height=465',
              );
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
