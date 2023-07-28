import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useTopics } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FlexboxTable,
  FormikSelect,
  FormikText,
  FormPage,
  getEnumStringOptions,
  ITopicModel,
  Row,
  TopicTypeName,
} from 'tno-core';

import { Columns, defaultTopic } from './constants';
import * as styled from './styled';
import { TopicFilter } from './TopicFilter';

/**
 * Provides a list of all topics.
 * Provides CRUD form for topics.
 * @returns Component
 */
export const TopicList: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [, api] = useTopics();

  const [loading, setLoading] = React.useState(false);
  const [topics, setTopics] = React.useState<ITopicModel[]>([]);
  const [items, setItems] = React.useState<ITopicModel[]>([]);
  const [topic, setTopic] = React.useState<ITopicModel>(defaultTopic);

  const topicId = Number(id);
  const topicTypeOptions = getEnumStringOptions(TopicTypeName);

  React.useEffect(() => {
    if (!items.length && !loading) {
      setLoading(true);
      api
        .findAllTopics()
        .then((data) => {
          setTopics(data.filter((t) => t.id !== 1));
          setItems(data.filter((t) => t.id !== 1 && t.isEnabled));
        })
        .finally(() => {
          setLoading(false);
        });
    }
  }, [api, items.length, loading]);

  React.useEffect(() => {
    changeParentOverflow();
  }, []);

  React.useEffect(() => {
    const topic = items.find((i) => i.id === topicId) ?? defaultTopic;
    setTopic(topic);
  }, [topicId, items]);

  const handleRemove = async (topicId: number) => {
    const topic = items.find((i) => i.id === topicId);
    if (!topic) return;
    const removedTopic = { ...topic, isEnabled: false };
    await api.updateTopic(removedTopic);
    setItems(items.filter((t) => t.id !== topic.id));
    setTopics(topics.map((item) => (item.id === topic.id ? removedTopic : item)));
    if (!!id) navigate('/admin/topics');
    toast.success(`${topic.name} has successfully been deleted.`);
  };

  const handleSubmit = async (values: ITopicModel) => {
    try {
      var results = [...items];
      let updatedTopics: ITopicModel[];
      const existsTopic = topics.find((x) => x.name === values.name);
      if (values.id === 0) {
        if (!existsTopic) {
          const result = await api.addTopic(values);
          results = [...items, result];
          updatedTopics = [...topics, result];
        } else {
          if (existsTopic.isEnabled) {
            toast.warn(`${values.name} already exists.`);
            return;
          } else {
            const result = await api.updateTopic({
              ...existsTopic,
              isEnabled: values.isEnabled,
              topicType: values.topicType,
            });
            results = [...items, result];
            updatedTopics = topics.map((i) => (i.id === existsTopic.id ? result : i));
          }
        }
      } else {
        if (!existsTopic || values.id === existsTopic.id) {
          const result = await api.updateTopic(values);
          results = items.map((i) => (i.id === values.id ? result : i));
          updatedTopics = topics.map((i) => (i.id === topic.id ? result : i));
        } else {
          toast.warn(`${values.name} already exists.`);
          return;
        }
      }
      setItems(results.filter((x) => x.isEnabled));
      setTopics(updatedTopics);
      toast.success(`${values.name} has successfully been saved.`);
      // Do we have a better way here for the clean-up after adding a new topic?
      if (!id || id === '0') {
        values.name = defaultTopic.name;
        values.topicType = defaultTopic.topicType;
        if (id === '0') navigate('/admin/topics');
      }
    } catch {
      // Ignore error as it's handled globally.
    }
  };

  const changeParentOverflow = () => {
    let mainElement = document.querySelector('main');
    (mainElement as any).style.overflowY = 'hidden';
  };

  return (
    <styled.TopicList>
      <FormPage>
        <Col flex="2 1 0">
          <Col>
            <TopicFilter
              onFilterChange={(filter) => {
                if (filter && filter.length) {
                  const value = filter.toLocaleLowerCase();
                  setItems(
                    topics.filter(
                      (i) =>
                        i.isEnabled &&
                        (i.name.toLocaleLowerCase().includes(value) ||
                          i.description.toLocaleLowerCase().includes(value) ||
                          i.topicType.toLocaleLowerCase().includes(value)),
                    ),
                  );
                } else {
                  setItems(topics.filter((x) => x.isEnabled));
                }
              }}
            />
            <FormikForm
              loading={false}
              initialValues={topic}
              onSubmit={async (values, { setSubmitting }) => {
                await handleSubmit(values);
                setSubmitting(false);
              }}
            >
              {({ isSubmitting, values, setValues }) => (
                <>
                  <Row justifyContent="space-between">
                    <Row>
                      <FormikText name="name" label="Name" width="208px" />
                      <FormikSelect
                        label="Type"
                        name="topicType"
                        options={topicTypeOptions}
                        value={topicTypeOptions.find((o) => o.value === values.topicType)}
                        width={FieldSize.Medium}
                      />
                    </Row>
                    <Row alignItems="self-end" style={{ paddingBottom: '7px' }}>
                      <Button
                        type="submit"
                        variant={ButtonVariant.primary}
                        disabled={
                          isSubmitting || !values.name || !values.topicType || (!!id && id !== '0')
                        }
                      >
                        Save
                      </Button>
                      <Button
                        disabled={isSubmitting || id === '0'}
                        onClick={() => {
                          navigate('/admin/topics/0');
                        }}
                      >
                        Create New Issue/Topic
                      </Button>
                    </Row>
                  </Row>
                </>
              )}
            </FormikForm>
            <FlexboxTable
              rowId="id"
              data={items}
              columns={Columns(handleRemove, handleSubmit)}
              showSort={true}
              activeRowId={id}
              onRowClick={(row) => navigate(`/admin/topics/${row.original.id}`)}
              pagingEnabled={false}
            />
          </Col>
        </Col>
      </FormPage>
    </styled.TopicList>
  );
};
