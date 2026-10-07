import React from 'react';
import Select from 'react-select';
import { useLookup } from 'store/hooks';
import { type ITopicModel, TopicTypeName } from 'tno-core';

import {
  type IGroupedTopicOptions,
  type ITopicOptionItem,
} from '../../features/content/form/interfaces';
import * as styled from './styled';

export interface ITopicProps {
  name: string;
  className: string;
  value: number | undefined;
  filteredTopics?: ITopicModel[] | undefined;
  isDisabled: boolean;
  handleTopicChange?: (value: ITopicModel | undefined) => void;
}

/**
 * A form component to enter the content topic.
 * @returns Form component for topic.
 */
export const Topic: React.FC<ITopicProps> = ({
  name,
  className,
  value,
  filteredTopics,
  isDisabled,
  handleTopicChange,
}) => {
  const [{ topics }] = useLookup();
  const [groupedOptions, setGroupedOptions] = React.useState<IGroupedTopicOptions[]>([]);

  // The system [Not Applicable] topic is identified by its flag, not by its id or name.
  const topicIdNotApplicable = (filteredTopics ?? topics)?.find((t) => t.isSystem)?.id;

  React.useEffect(() => {
    if (filteredTopics) {
      setGroupedOptions(convertToGroupedOptions(filteredTopics));
    } else if (topics) {
      setGroupedOptions(convertToGroupedOptions(topics));
    }
    // It's safe to ignore `convertToGroupedOptions`
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [topics, filteredTopics]);

  const convertToGroupedOptions = (topics: ITopicModel[]): IGroupedTopicOptions[] => {
    const groupedOptions: IGroupedTopicOptions[] = [];
    const notApplicableTopic = topics.find((el) => el.isSystem);
    if (notApplicableTopic) {
      groupedOptions.push({
        label: notApplicableTopic.name,
        options: [
          {
            isDisabled: false,
            label: notApplicableTopic.name,
            topicType: TopicTypeName.Issues,
            value: notApplicableTopic.id,
          } as ITopicOptionItem,
        ],
      });
    }
    const topicNames = Object.keys(TopicTypeName);
    // reverse the sort here because the customer wants the second enum first
    topicNames
      .slice()
      .reverse()
      .forEach((key) => {
        let filteredTopics = topics.filter(
          (el) =>
            !el.isSystem &&
            el.topicType === key &&
            // show all enabled Topics or disabled Topic if it's set as current
            (el.isEnabled || (!el.isEnabled && el.id === value)),
        );
        if (filteredTopics) {
          filteredTopics = filteredTopics.sort((a, b) => {
            // sort by Topic Name
            return a.name.localeCompare(b.name);
          });
        }
        groupedOptions.push({
          label: key,
          options: filteredTopics.map(
            (t) =>
              ({
                isDisabled: !t.isEnabled,
                label: t.name,
                topicType: t.topicType,
                value: t.id,
              } as ITopicOptionItem),
          ),
        });
      });
    return groupedOptions;
  };

  const getTopicOption = (targetTopicId: number): any => {
    for (let groupCounter = 0; groupCounter < groupedOptions.length; groupCounter++) {
      for (
        let optionCounter = 0;
        optionCounter < groupedOptions[groupCounter].options.length;
        optionCounter++
      ) {
        if (groupedOptions[groupCounter].options[optionCounter].value === targetTopicId) {
          return groupedOptions[groupCounter].options[optionCounter];
        }
      }
    }
  };

  const formatOptionLabel = (data: ITopicOptionItem) => (
    <div
      className={
        (data.value === topicIdNotApplicable ? 'type-not-applicable' : `type-${data.topicType}`) +
        // This extra style exists only to flag disabled topics that are disabled.
        // These could show up because of migration from TNO, or through changes to
        // content and topics that are possible
        (data.isDisabled ? ' type-disabled' : '')
      }
    >
      <span
        className={
          'option-hint ' +
          (data.value === topicIdNotApplicable ? 'type-not-applicable' : `type-${data.topicType}`)
        }
      >
        {data.topicType.charAt(0)}
      </span>
      {data.label}
    </div>
  );

  return (
    <styled.Topic>
      <Select
        name={name}
        options={groupedOptions}
        isDisabled={isDisabled}
        isClearable={false}
        className={className}
        value={getTopicOption(value ?? topicIdNotApplicable ?? 0)}
        onChange={(e: any) => {
          let value;
          if (e?.value) {
            if (filteredTopics) {
              value = filteredTopics.find((c) => c.id === e.value);
            } else {
              value = topics.find((c) => c.id === e.value);
            }
          }
          handleTopicChange?.(value || undefined);
        }}
        formatOptionLabel={formatOptionLabel}
        styles={{
          control: (provided, state) => ({
            ...provided,
            border: '1px solid rgb(96, 96, 96)',
          }),
        }}
        menuPosition={'fixed'}
      />
    </styled.Topic>
  );
};
