import { getPreviewReportRoute } from 'features/content';
import React from 'react';
import { FaBinoculars } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { useFolders, useTopics } from 'store/hooks/admin';
import {
  Button,
  FlexboxTable,
  FormPage,
  getSortableOptions,
  IContentTopicModel,
  IFolderContentModel,
  IOptionItem,
  ITopicModel,
  OptionItem,
  Row,
  Settings,
  TopicTypeName,
} from 'tno-core';

import { TopicFormSmall } from '../topics';
import { useColumns } from './hooks';
import * as styled from './styled';

// item with id of 1 is the magic [Not Applicable] topic
const topicIdNotApplicable = 1;

export interface IGroupedOption {
  readonly label: string;
  readonly options: readonly IOptionItem<string | number | undefined>[];
}

/**
 * Provides a list of all topics.
 * Provides CRUD form for topics.
 * @returns Component
 */
const EventOfTheDayList: React.FC = () => {
  const [{ isReady, settings }] = useLookup();
  const [, { findAllTopics, updateTopic, addTopic }] = useTopics();

  const [, { getContentInFolder }] = useFolders();
  const [, { updateContentTopics }] = useContent();

  const [eventOfTheDayFolderId, setEventOfTheDayFolderId] = React.useState(0);
  const [eventOfTheDayReportId, setEventOfTheDayReportId] = React.useState(0);
  const [items, setItems] = React.useState<IFolderContentModel[]>([]);
  const [allTopics, setAllTopics] = React.useState<ITopicModel[]>([]);
  const [groupedOptions, setGroupedOptions] = React.useState<IGroupedOption[]>([]);

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
        setGroupedOptions(convertToGroupedOptions(data));
      })
      .catch(() => {});
    // KGM - overridden to enforce only call once
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [eventOfTheDayFolderId]);

  const handleSubmit = async (values: IFolderContentModel) => {
    try {
      var result: IContentTopicModel[];
      if (values.content!.topics[0].id === topicIdNotApplicable)
        result = await updateContentTopics(values.contentId, undefined);
      else result = await updateContentTopics(values.contentId, values.content!.topics);

      let index = items.findIndex((el) => el.contentId === values.contentId);
      let results = [...items];
      results[index].content!.topics = result;
      setItems(results);
    } catch {
      // Ignore error as it's handled globally.
    }
  };

  const formatTopicOption = (item: ITopicModel): OptionItem => {
    return new OptionItem(
      (
        <div
          className={
            (item.id === topicIdNotApplicable ? `type-not-applicable` : `type-${item.topicType}`) +
            // This extra style exists only to flag disabled topics that are disabled.
            // These could show up because of migration from TNO, or through changes to
            // content and topics that are possible
            (!item.isEnabled ? ' type-disabled' : '')
          }
        >
          {item.name}
        </div>
      ),
      item.id,
      !item.isEnabled,
    );
  };

  const convertToGroupedOptions = (topics: ITopicModel[]): IGroupedOption[] => {
    const groupedOptions: IGroupedOption[] = [];
    const notApplicableTopic = topics.find((el) => el.id === topicIdNotApplicable);
    if (notApplicableTopic)
      groupedOptions.push({
        label: 'Not Applicable',
        options: getSortableOptions([notApplicableTopic], undefined, undefined, (item) =>
          formatTopicOption(item),
        ),
      });
    const topicNames = Object.keys(TopicTypeName);
    // reverse the sort here because the customer wants the secon enum first
    topicNames
      .slice()
      .reverse()
      .forEach((key) => {
        let filteredTopics = topics.filter(
          (el) => el.id !== topicIdNotApplicable && el.topicType === key,
        );
        if (filteredTopics)
          groupedOptions.push({
            label: key,
            options: getSortableOptions(
              filteredTopics,
              undefined,
              undefined,
              (item) => formatTopicOption(item),
              (a, b) => {
                // sort Topic Name
                return a.name.localeCompare(b.name);
              },
            ),
          });
      });
    return groupedOptions;
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
      setGroupedOptions(convertToGroupedOptions(results));
    } catch {
      // Ignore error as it's handled globally.
    }
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
        <Row className="topic-form-row">
          <TopicFormSmall onAddOrUpdate={handleAddOrUpdate} />
        </Row>
        <FlexboxTable
          rowId="contentId"
          data={items}
          columns={useColumns(handleSubmit, allTopics, groupedOptions)}
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
