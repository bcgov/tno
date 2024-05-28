import { formatDate } from 'features/utils';
import moment from 'moment';
import * as React from 'react';
import { useLookup, useSettings } from 'store/hooks';
import { ContentTypeName, IContentModel, Row, Show } from 'tno-core';

export interface IAttributesProps {
  /** The content item */
  item: IContentModel;
  /** Whether to show the date */
  showDate?: boolean;
  /** Whether to show the time */
  showTime?: boolean;
  /** Whether to show series */
  showSeries?: boolean;
  /** The view options */
  viewOptions?: {
    section: boolean;
  };
  /** Whether to this is the mobile view */
  mobile?: boolean;
}
export const Attributes: React.FC<IAttributesProps> = ({
  item,
  showDate,
  showTime,
  showSeries,
  viewOptions,
  mobile,
}) => {
  const [{ mediaTypes }] = useLookup();
  const { excludeBylineIds, excludeSourceIds } = useSettings();
  return (
    <Row className={`${mobile && 'mobile add-margin'} attributes`}>
      {showDate && <div className="date has-divider">{formatDate(item.publishedOn)}</div>}
      {showTime && item.contentType !== ContentTypeName.PrintContent && (
        <div className="time has-divider">{`${moment(item.publishedOn).format('HH:mm')}`}</div>
      )}
      {item.source &&
        !excludeSourceIds?.some((mt) => {
          const mediaTypeObj = mediaTypes.find((m) => m.id === mt);
          return item.mediaTypeId === mediaTypeObj?.id;
        }) && (
          <div
            className={`source ${showSeries && item.series && 'has-divider'}`}
          >{`${item.source.name}`}</div>
        )}
      {/* do not include byline in talk radio, news radio, or events even if present */}
      {item.byline &&
        !excludeBylineIds?.some((mt) => {
          const mediaTypeObj = mediaTypes.find((m) => m.id === mt);
          return item.mediaTypeId === mediaTypeObj?.id;
        }) && <div className={`byline`}>{`${item.byline}`}</div>}
      {/* show series when source not shown and the show series flag is disabled */}
      {item.series &&
        !showSeries &&
        excludeSourceIds?.some((mt) => {
          const mediaTypeObj = mediaTypes.find((m) => m.id === mt);
          return item.mediaTypeId === mediaTypeObj?.id;
        }) && <div className="series">{item.series.name}</div>}
      {/* show series when show series flag is enabled */}
      {item.series && showSeries && (
        <div className={`series ${!!item.section && 'has-divider'}`}>{item.series.name}</div>
      )}
      <Show visible={viewOptions?.section}>
        {item.section && <div className="section">{item.section}</div>}
        {item.page && <div className="page-number">{item.page}</div>}
      </Show>
    </Row>
  );
};
