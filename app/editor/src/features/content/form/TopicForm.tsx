import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import {
  FieldSize,
  filterEnabledOptions,
  FormikSelect,
  getSortableOptions,
  IOptionItem,
  Row,
  Text,
} from 'tno-core';

import { IContentForm } from './interfaces';
import { calcTopicScore } from './utils/calcTopicScore';

export interface ITopicFormProps {}

/**
 * A form component to enter the content topic.
 * @returns Form component for topic.
 */
export const TopicForm: React.FC<ITopicFormProps> = () => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [{ topics, rules }] = useLookup();

  const [topicOptions, setTopicOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setTopicOptions(getSortableOptions(topics));
  }, [topics]);

  return (
    <Row>
      <FormikSelect
        name="topics"
        label="Topic"
        width={FieldSize.Medium}
        options={filterEnabledOptions(
          topicOptions,
          !!values.topics?.length ? values.topics[0].id : undefined,
        )}
        required
        isDisabled={!values.sourceId}
        value={
          !!values.topics?.length ? topicOptions.find((c) => c.value === values.topics[0].id) : []
        }
        onChange={(e: any) => {
          let value;
          if (!!e?.value) {
            value = topics.find((c) => c.id === e.value);
          }
          setFieldValue(
            'topics',
            !!value ? [{ ...value, score: calcTopicScore(values, rules) }] : [],
          );
        }}
      />
      <Text
        name="score"
        label="Score"
        width={FieldSize.Tiny}
        maxLength={3}
        required={
          !!values.topics.length && values.topics[0].name.toLocaleLowerCase() !== 'not applicable'
        }
        value={values.topics.length ? values.topics[0].score : ''}
        disabled={
          values.topics.length === 0 ||
          values.topics[0].name.toLocaleLowerCase() === 'not applicable'
        }
        onChange={(e) => {
          const value = e.currentTarget.value;
          if (!isNaN(Number(value))) setFieldValue('topics.0.score', +value);
          else e.currentTarget.value = '';
        }}
      />
    </Row>
  );
};
