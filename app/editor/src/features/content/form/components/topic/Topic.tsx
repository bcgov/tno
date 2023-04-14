import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import {
  FieldSize,
  filterEnabledOptions,
  FormikSelect,
  getSortableOptions,
  IOptionItem,
  OptionItem,
  Row,
  Text,
  TopicTypeName,
} from 'tno-core';

import { IContentForm } from '../../interfaces';
import { calcTopicScore } from '../../utils/calcTopicScore';

export interface ITopicProps {}

/**
 * A form component to enter the content topic.
 * @returns Form component for topic.
 */
export const Topic: React.FC<ITopicProps> = () => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [{ topics, rules }] = useLookup();

  const [topicOptions, setTopicOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setTopicOptions(
      getSortableOptions(
        topics,
        [],
        (item) =>
          new OptionItem(
            (
              <div className={item.id > 1 ? `type-${item.topicType}` : 'type-none'}>
                {item.topicType === TopicTypeName.Issues
                  ? item.name
                  : `${item.name} (${item.topicType})`}
              </div>
            ),
            item.id,
            item.isEnabled,
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
      ),
    );
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
        filterOption={(option, input) => {
          const label = (option.label as any)?.props?.children?.toLowerCase();
          return !input || label?.includes(input.toLowerCase());
        }}
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
