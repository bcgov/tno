import { getPreviewReportRoute } from 'features/content';
import React from 'react';
import { FaBinoculars } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { useFolders } from 'store/hooks/admin';
import {
  Button,
  FlexboxTable,
  FormPage,
  getSortableOptions,
  IFolderContentModel,
  IOptionItem,
  OptionItem,
  Row,
  Settings,
  TopicTypeName,
} from 'tno-core';

import { useColumns } from './hooks';
import * as styled from './styled';

/**
 * Provides a list of all topics.
 * Provides CRUD form for topics.
 * @returns Component
 */
const EventOfTheDayList: React.FC = () => {
  const [{ isReady, settings, topics }] = useLookup();

  const [, { getContentInFolder }] = useFolders();
  const [, { updateContentTopics }] = useContent();

  const [eventOfTheDayFolderId, setEventOfTheDayFolderId] = React.useState(0);
  const [eventOfTheDayReportId, setEventOfTheDayReportId] = React.useState(0);
  const [items, setItems] = React.useState<IFolderContentModel[]>([]);
  const [topicOptions, setTopicOptions] = React.useState<
    IOptionItem<string | number | undefined>[]
  >([]);

  React.useEffect(() => {
    if (isReady) {
      const eventOfTheDayFolderId = settings.find(
        (s) => s.name === Settings.EventOfTheDayFolder,
      )?.value;
      if (eventOfTheDayFolderId) setEventOfTheDayFolderId(+eventOfTheDayFolderId);
      else toast.error(`${Settings.EventOfTheDayFolder} setting needs to be configured.`);
      const eventOfTheDayReportId = settings.find(
        (s) => s.name === Settings.EventOfTheDayReport,
      )?.value;
      if (eventOfTheDayReportId) setEventOfTheDayReportId(+eventOfTheDayReportId);
      else toast.error(`${Settings.EventOfTheDayReport} setting needs to be configured.`);
    }
  }, [isReady, settings]);

  React.useEffect(() => {
    if (eventOfTheDayFolderId) {
      getContentInFolder(eventOfTheDayFolderId, true)
        .then((data) => {
          setItems(data);
        })
        .catch(() => {})
        .finally(() => {});
    }
    setTopicOptions(getSortedTopicOptions());
    // KGM - overridden to enforce only call once
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [eventOfTheDayFolderId]);

  const handleSubmit = async (values: IFolderContentModel) => {
    try {
      const result = await updateContentTopics(values.contentId, values.content!.topics);
      let index = items.findIndex((el) => el.contentId === values.contentId);
      let results = [...items];
      results[index].content!.topics = result;
      setItems(results);
    } catch {
      // Ignore error as it's handled globally.
    }
  };

  const getSortedTopicOptions = () => {
    return getSortableOptions(
      topics,
      undefined,
      undefined,
      (item) =>
        new OptionItem(
          (
            <div
              className={
                (item.id > 1 ? `type-${item.topicType}` : 'type-none') +
                // This extra style exists only to flag disabled topics that are disabled.
                // These could show up because of migration from TNO, or through changes to
                // content and topics that are possible
                (!item.isEnabled ? ' type-disabled' : '')
              }
            >
              {item.topicType === TopicTypeName.Issues
                ? item.name
                : `${item.name} (${item.topicType})`}
            </div>
          ),
          item.id,
          !item.isEnabled,
        ),
      (a, b) => {
        if (a.topicType < b.topicType) return -1;
        if (a.topicType > b.topicType) return 1;
        if (a.sortOrder < b.sortOrder) return -1;
        if (a.sortOrder > b.sortOrder) return 1;
        if (a.name < b.name) return -1;
        if (a.name > b.name) return 1;
        return 0;
      },
    );
  };

  return (
    <styled.EventOfTheDayList>
      <FormPage>
        <Row className="page-header">
          <p className="list-title">Update Content for Event of the Day</p>
          <div className="buttons">
            <Button
              onClick={() =>
                window.open(
                  `${getPreviewReportRoute(eventOfTheDayReportId)}?showNav=false`,
                  '_blank',
                )
              }
            >
              Preview <FaBinoculars className="icon" />
            </Button>
          </div>
        </Row>
        <FlexboxTable
          rowId="contentId"
          data={items}
          columns={useColumns(handleSubmit, topics, topicOptions)}
          groupBy={(item) => {
            if (item.original.content?.series?.name) return item.original.content?.series?.name;
            else if (item.original.content?.source?.name)
              return item.original.content?.source?.name;
            else return ' ';
          }}
          showActive={false}
          showSort={false}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.EventOfTheDayList>
  );
};

export default EventOfTheDayList;
