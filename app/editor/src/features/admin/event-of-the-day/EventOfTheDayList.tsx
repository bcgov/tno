import { getPreviewReportRoute } from 'features/content';
import React from 'react';
import { FaBinoculars, FaPaperPlane } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { useFolders, useReports, useTopics } from 'store/hooks/admin';
import {
  Button,
  FlexboxTable,
  FormPage,
  IContentTopicModel,
  IFolderContentModel,
  IReportModel,
  ITopicModel,
  Modal,
  Row,
  Settings,
  useModal,
} from 'tno-core';

import { TopicFormSmall } from '../topics';
import { useColumns } from './hooks';
import * as styled from './styled';

/**
 * Provides a list of all topics.
 * Provides CRUD form for topics.
 * @returns Component
 */
const EventOfTheDayList: React.FC = () => {
  const [{ isReady, settings }] = useLookup();
  const [, { findAllTopics, updateTopic, addTopic }] = useTopics();
  const [, { getReport, publishReport }] = useReports();

  const { toggle, isShowing } = useModal();

  const [, { getContentInFolder }] = useFolders();
  const [, { updateContentTopics }] = useContent();

  const [eventOfTheDayFolderId, setEventOfTheDayFolderId] = React.useState(0);
  const [eventOfTheDayReportId, setEventOfTheDayReportId] = React.useState(0);
  const [items, setItems] = React.useState<IFolderContentModel[]>([]);
  const [allTopics, setAllTopics] = React.useState<ITopicModel[]>([]);

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

  const sortBySourceSortOrderAndPage = (data: IFolderContentModel[]) => {
    data = data.sort((a, b) => {
      // apply some default sortOrder and page values where neccesary for comparison
      const firstItemSourceSortOrder = a.content?.source?.sortOrder ?? 99999;
      const firstItemPage = a.content?.page ?? 'ZZZ';
      const secondItemSourceSortOrder = b.content?.source?.sortOrder ?? 99999;
      const secondItemPage = a.content?.page ?? 'ZZZ';
      if (firstItemSourceSortOrder === secondItemSourceSortOrder) {
        // Page is only important when Source.SortOrder are the same
        return firstItemPage.localeCompare(secondItemPage);
      }
      return firstItemSourceSortOrder > secondItemSourceSortOrder ? 1 : -1;
    });
    return data;
  };

  React.useEffect(() => {
    if (eventOfTheDayFolderId) {
      getContentInFolder(eventOfTheDayFolderId, true)
        .then((data) => {
          data = sortBySourceSortOrderAndPage(data);
          setItems(data);
        })
        .catch(() => {})
        .finally(() => {});
    }
    findAllTopics()
      .then((data) => {
        setAllTopics(data);
      })
      .catch(() => {});
    // KGM - overridden to enforce only call once
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [eventOfTheDayFolderId]);

  const handleSubmit = async (values: IFolderContentModel) => {
    try {
      var result: IContentTopicModel[] = await updateContentTopics(
        values.contentId,
        values.content!.topics,
      );

      let index = items.findIndex((el) => el.contentId === values.contentId);
      let results = [...items];
      results[index].content!.topics = result;
      setItems(results);
    } catch {
      // Ignore error as it's handled globally.
    }
  };

  const handleAddOrUpdate = async (values: ITopicModel) => {
    try {
      let results: ITopicModel[] = [];
      // need case insensitive string compare here or we will end up with variations on names
      const topicNameMatch = allTopics.find(
        (x) => x.name.toUpperCase() === values.name.toUpperCase(),
      );

      if (values.id === 0) {
        if (!topicNameMatch) {
          const result = await addTopic(values);
          results = [...allTopics, result];
          toast.success(`Topic with name [${values.name}] has been added.`);
        } else {
          if (topicNameMatch.isEnabled) {
            toast.warn(`Topic with name [${values.name}] already exists.`);
            return;
          } else {
            const result = await updateTopic({
              ...topicNameMatch,
              isEnabled: values.isEnabled,
              topicType: values.topicType,
            });
            results = [...allTopics, result];
            toast.success(`Topic with name [${values.name}] has been added.`);
          }
        }
      }
      setAllTopics(results);
    } catch {
      // Ignore error as it's handled globally.
    }
  };

  const handlePublish = async () => {
    try {
      await getReport(eventOfTheDayReportId).then((report: IReportModel) => publishReport(report));
      toast.success('Event of the Day report has been successfully requested.');
    } catch {}
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
            <Button onClick={() => toggle()}>
              Send <FaPaperPlane className="icon" />
            </Button>
          </div>
        </Row>
        <Row className="topic-form-row">
          <TopicFormSmall onAddOrUpdate={handleAddOrUpdate} />
        </Row>
        <FlexboxTable
          rowId="contentId"
          data={items}
          columns={useColumns(allTopics, handleSubmit)}
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
        <Row className="page-header">
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
            <Button onClick={() => toggle()}>
              Send <FaPaperPlane className="icon" />
            </Button>
          </div>
        </Row>
      </FormPage>
      <Modal
        headerText="Confirm Send"
        body="Are you sure you wish to send the Event of the Day report?"
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, Send"
        onConfirm={async () => {
          try {
            await handlePublish();
          } catch {
            // Globally handled
          } finally {
            toggle();
          }
        }}
      />
    </styled.EventOfTheDayList>
  );
};

export default EventOfTheDayList;
