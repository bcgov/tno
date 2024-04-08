import React from 'react';
import Select from 'react-select';
import { useLookup } from 'store/hooks';
import { ITopicModel, TopicTypeName } from 'tno-core';

import { IGroupedTopicOptions } from '../../features/content/form/interfaces';
import { ITopicOptionItem } from '../../features/content/form/interfaces';
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

  // item with id of 1 is the magic [Not Applicable] topic
  const topicIdNotApplicable = 1;

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
    const notApplicableTopic = topics.find((el) => el.id === topicIdNotApplicable);
    if (notApplicableTopic)
      groupedOptions.push({
        label: 'Not Applicable',
        options: [
          {
            isDisabled: false,
            label: 'Not Applicable',
            topicType: TopicTypeName.Issues,
            value: topicIdNotApplicable,
          } as ITopicOptionItem,
        ],
      });
    const topicNames = Object.keys(TopicTypeName);
    // reverse the sort here because the customer wants the second enum first
    topicNames
      .slice()
      .reverse()
      .forEach((key) => {
        let filteredTopics = topics.filter(
          (el) =>
            el.id !== topicIdNotApplicable &&
            el.topicType === key &&
            // show all enabled Topics or disabled Topic if it's set as current
            (el.isEnabled || (!el.isEnabled && el.id === value)),
        );
        if (filteredTopics)
          filteredTopics = filteredTopics.sort((a, b) => {
            // sort by Topic Name
            return a.name.localeCompare(b.name);
          });
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
    for (var groupCounter = 0; groupCounter < groupedOptions.length; groupCounter++) {
      for (
        var optionCounter = 0;
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
        (data.value === topicIdNotApplicable ? `type-not-applicable` : `type-${data.topicType}`) +
        // This extra style exists only to flag disabled topics that are disabled.
        // These could show up because of migration from TNO, or through changes to
        // content and topics that are possible
        (data.isDisabled ? ' type-disabled' : '')
      }
    >
      <span
        className={
          `option-hint ` +
          (data.value === topicIdNotApplicable ? `type-not-applicable` : `type-${data.topicType}`)
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
        value={getTopicOption(value ?? topicIdNotApplicable)}
        onChange={(e: any) => {
          let value;
          if (!!e?.value) {
            if (filteredTopics) {
              value = filteredTopics.find((c) => c.id === e.value);
            } else {
              value = topics.find((c) => c.id === e.value);
            }
          }
          handleTopicChange?.(!!value ? value : undefined);
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
