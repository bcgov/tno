import { useFormikContext } from 'formik';
import React from 'react';
import { FaPlus } from 'react-icons/fa';
import { createFilter } from 'react-select';
import { useLookup } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  FieldSize,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  type IIngestModel,
  OptionItem,
  Row,
  Show,
} from 'tno-core';

import { type ISyndicationFeed } from '../interfaces';
import { TimeZones } from './constants';
import { FeedList } from './FeedList';
import { ImportContent } from './ImportContent';
import { SourceMapping } from './SourceMapping';
import * as styled from './styled';

export const Syndication: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const [lookups] = useLookup();
  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);

  const sourceOptions = React.useMemo(
    () => lookups.sources.map((s) => new OptionItem(`${s.code} - ${s.name}`, s.code, !s.isEnabled)),
    [lookups.sources],
  );
  // Find a source by its code, name or short name.
  const sourceFilter = React.useMemo(() => {
    const search = new Map(
      lookups.sources.map((s) => [
        s.code,
        [s.code, s.name, s.shortName].filter((v) => !!v).join(' '),
      ]),
    );
    return createFilter<unknown>({
      stringify: (option) => search.get(option.value) ?? `${option.label} ${option.value}`,
    });
  }, [lookups.sources]);
  const feedTable = React.useRef<HTMLDivElement>(null);
  const scrollToNewFeed = React.useRef(false);
  const feeds: ISyndicationFeed[] = React.useMemo(
    () => (Array.isArray(values.configuration.feeds) ? values.configuration.feeds : []),
    [values.configuration.feeds],
  );

  const updateFeeds = React.useCallback(
    (updated: ISyndicationFeed[]) => setFieldValue('configuration.feeds', updated),
    [setFieldValue],
  );

  const handleAddFeed = () => {
    // Keep the single feed URL as the first feed when switching to a list of feeds.
    const current: ISyndicationFeed[] =
      feeds.length === 0 && values.configuration.url
        ? [{ url: values.configuration.url, source: '' }]
        : feeds;
    scrollToNewFeed.current = true;
    updateFeeds([...current, { url: '', source: '' }]);
  };

  // Bring a new feed into view at the bottom of the scrolling list.
  React.useEffect(() => {
    if (!scrollToNewFeed.current) return;
    scrollToNewFeed.current = false;
    window.requestAnimationFrame(() => {
      const rows = feedTable.current?.querySelector('.feed-rows');
      if (rows) rows.scrollTop = rows.scrollHeight;
    });
  }, [feeds.length]);

  return (
    <styled.IngestType>
      <ImportContent />
      <b>Feeds</b>
      <Show visible={feeds.length === 0}>
        <FormikText label="Syndication Feed URL" name="configuration.url" required />
      </Show>
      <Show visible={feeds.length > 0}>
        <p>
          Feeds are imported in priority order. Stories from a feed are assigned its source, or the
          ingest source mapping and source when it has none.
        </p>
        <FeedList
          ref={feedTable}
          feeds={feeds}
          sources={sourceOptions}
          sourceFilter={sourceFilter}
          onChange={updateFeeds}
        />
      </Show>
      <Row>
        <Button variant={ButtonVariant.secondary} onClick={handleAddFeed}>
          <FaPlus /> Add feed
        </Button>
      </Row>
      <FormikCheckbox
        label="Skip duplicate stories"
        name="configuration.dedupEnabled"
        tooltip="Skip a new story when a story from the same or a higher priority feed was published recently with the same or a similar headline, or nearly the same text."
      />
      <Show visible={!!values.configuration.dedupEnabled}>
        <Row gap="1rem">
          <FormikText
            label="Compare stories within (days)"
            name="configuration.dedupWindowDays"
            type="number"
            min={1}
            max={90}
            placeholder="7"
            width={FieldSize.Small}
            tooltip="Only stories published within this many days of each other are compared. Defaults to 7."
          />
          <FormikText
            label="Headline similarity"
            name="configuration.dedupHeadlineThreshold"
            type="number"
            min={0.01}
            max={1}
            step={0.01}
            placeholder="0.8"
            width={FieldSize.Small}
            tooltip="Share of headline words two stories must have in common, from 0 to 1 (1 is the same words). Defaults to 0.8."
          />
          <FormikText
            label="Story text similarity"
            name="configuration.dedupBodyThreshold"
            type="number"
            min={0.5}
            max={1}
            step={0.01}
            placeholder="0.85"
            width={FieldSize.Small}
            tooltip="How alike the text of two stories must be, from 0.5 (unrelated) to 1 (identical). Defaults to 0.85."
          />
        </Row>
      </Show>
      <FormikCheckbox
        label="Custom RSS/ATOM"
        name="configuration.customFeed"
        tooltip="If the feed doesn't follow the standardized rules it is a custom feed."
      />
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        value={timeZone}
      />
      <FormikText
        label="Username"
        name="configuration.username"
        tooltip="Configure in the source connection"
      />
      <FormikText
        label="Password"
        name="configuration.password"
        tooltip="Configure in the source connection"
        type="password"
        autoComplete="off"
      />
      <FormikCheckbox
        label="Fetch Content Body Separately"
        name="configuration.fetchContent"
        tooltip="Whether content body is located remotely"
        onChange={(e) => {
          setFieldValue('configuration.fetchContent', e.currentTarget.checked);
        }}
      />
      <SourceMapping />
    </styled.IngestType>
  );
};
