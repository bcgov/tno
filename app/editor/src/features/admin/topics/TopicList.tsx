import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useTopics } from 'store/hooks/admin';
import { FlexboxTable, ITopicModel } from 'tno-core';

import { defaultTopic } from './constants';
import { useColumns } from './hooks';
import * as styled from './styled';
import { TopicFilter } from './TopicFilter';
import { TopicFormSmall } from './TopicFormSmall';

/**
 * Provides a list of all topics.
 * Provides CRUD form for topics.
 * @returns Component
 */
const TopicList: React.FC = () => {
  const navigate = useNavigate();
  const [, { findAllTopics, updateTopic, addTopic }] = useTopics();

  const [loading, setLoading] = React.useState(false);
  const [allTopics, setAllTopics] = React.useState<ITopicModel[]>([]);
  const [filteredTopics, setFilteredTopics] = React.useState<ITopicModel[]>([]);
  const [topicFilter, setTopicFilter] = React.useState('');

  React.useEffect(() => {
    if (!filteredTopics.length && !loading && !topicFilter) {
      setLoading(true);
      findAllTopics()
        .then((data) => {
          setAllTopics(data.filter((t) => t.id !== 1));
          setFilteredTopics(
            data
              .filter((t) => t.id !== 1 && t.isEnabled)
              .sort(function (a, b) {
                // sort by Topic Type then Topic Name
                return b.topicType.localeCompare(a.topicType) || a.name.localeCompare(b.name);
              }),
          );
        })
        .catch(() => {})
        .finally(() => {
          setLoading(false);
        });
    }
  }, [findAllTopics, filteredTopics.length, loading, topicFilter]);

  const handleRemove = async (topicId: number) => {
    const topic = filteredTopics.find((i) => i.id === topicId);
    if (!topic) return;
    try {
      setLoading(true);
      const removedTopic = await updateTopic({ ...topic, isEnabled: false });
      setAllTopics(allTopics.map((item) => (item.id === topic.id ? removedTopic : item)));
      setFilteredTopics(filteredTopics.filter((t) => t.id !== topic.id));
      navigate('/admin/topics');
      toast.success(`Topic with name [${topic.name}] has been deleted.`);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (values: ITopicModel) => {
    try {
      setLoading(true);
      let results = [];
      let updatedTopics: ITopicModel[];
      // need case insensitive string compare here or we will end up with variations on names
      const topicNameMatch = allTopics.find(
        (x) => x.name.toUpperCase() === values.name.toUpperCase(),
      );

      if (values.id === 0) {
        if (!topicNameMatch) {
          const result = await addTopic(values);
          results = [...filteredTopics, result];
          updatedTopics = [...allTopics, result];
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
            results = [...filteredTopics, result];
            updatedTopics = allTopics.map((i) => (i.id === topicNameMatch.id ? result : i));
          }
        }
      } else {
        if (!topicNameMatch || values.id === topicNameMatch.id) {
          const result = await updateTopic(values);
          results = filteredTopics.map((i) => (i.id === values.id ? result : i));
          updatedTopics = allTopics.map((i) => (i.id === values.id ? result : i));
        } else {
          toast.warn(`Topic with name [${values.name}] already exists.`);
          return;
        }
      }
      setFilteredTopics(
        results
          .filter((x) => x.isEnabled)
          .sort(function (a, b) {
            // sort by Topic Type then Topic Name
            return b.topicType.localeCompare(a.topicType) || a.name.localeCompare(b.name);
          }),
      );
      setAllTopics(updatedTopics);
      toast.success(`Topic with name [${values.name}] has been saved.`);

      // Do we have a better way here for the clean-up after adding a new topic?
      if (values.id === 0) {
        if (values.name !== defaultTopic.name) values.name = defaultTopic.name;
        if (values.topicType !== defaultTopic.topicType) values.topicType = defaultTopic.topicType;
      }
    } catch {
      // Ignore error as it's handled globally.
    } finally {
      setLoading(false);
    }
  };

  return (
    <styled.TopicList>
      <FormPage>
        <p className="list-title">Update Topics List (Event of the Day)</p>
        <TopicFormSmall onAddOrUpdate={handleSubmit}></TopicFormSmall>
        <TopicFilter
          onFilterChange={(filter) => {
            setTopicFilter(filter);
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setFilteredTopics(
                allTopics.filter(
                  (i) =>
                    i.isEnabled &&
                    (i.name.toLocaleLowerCase().includes(value) ||
                      i.description.toLocaleLowerCase().includes(value) ||
                      i.topicType.toLocaleLowerCase().includes(value)),
                ),
              );
            } else {
              setFilteredTopics(allTopics.filter((x) => x.isEnabled));
            }
          }}
        />
        <FlexboxTable
          rowId="id"
          data={filteredTopics}
          columns={useColumns(handleRemove, handleSubmit, loading)}
          showSort={false}
          pagingEnabled={false}
          isLoading={loading}
        />
        {/* </Col>
        </Col> */}
      </FormPage>
    </styled.TopicList>
  );
};

export default TopicList;
