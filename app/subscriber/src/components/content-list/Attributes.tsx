import { formatDate } from 'features/utils';
import moment from 'moment';
import * as React from 'react';
import { useLookup } from 'store/hooks';
import { ContentTypeName, IContentModel, Row, Show } from 'tno-core';

export interface IAttributesProps {
  /** The content item */
  item: IContentModel;
  /** Whether to show the date */
  showDate?: boolean;
  /** Whether to show the time */
  showTime?: boolean;
  /** The view options */
  viewOptions?: {
    section: boolean;
  };
  /** Whether to add margin spacing */
  margin?: boolean;
}
export const Attributes: React.FC<IAttributesProps> = ({
  item,
  showDate,
  showTime,
  viewOptions,
  margin,
}) => {
  const noByLine = ['Talk Radio', 'News Radio', 'Events'];
  const noSource = ['Events', 'Talk Radio'];

  const [{ mediaTypes }] = useLookup();

  return (
    <Row className={`${margin && 'add-margin'}`}>
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
        }) && <div className={`byline ${!!item.section && 'has-divider'}`}>{`${item.byline}`}</div>}
      {item.series &&
        noSource.some((mt) => {
          const mediaTypeObj = mediaTypes.find((m) => m.name === mt);
          return item.mediaTypeId === mediaTypeObj?.id;
        }) && <div className="series">{item.series.name}</div>}
      <Show visible={viewOptions?.section}>
        {item.section && <div className="section">{item.section}</div>}
        {item.page && <div className="page-number">{item.page}</div>}
      </Show>
    </Row>
  );
};
