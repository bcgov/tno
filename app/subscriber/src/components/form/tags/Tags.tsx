import React from 'react';
import { FaTags } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { Col, FieldSize, IOptionItem, ITagModel, Row, Select, useWindowSize } from 'tno-core';

import * as styled from './styled';

export interface ITagsProps {
  /** The current content that is being viewed. */
  contentTags: ITagModel[] | [];
}

/**
 * The component that renders tags for a given text field
 * @returns the Tags component
 */
export const Tags: React.FC<ITagsProps> = ({ contentTags }) => {
  const [{ tags }] = useLookup();
  const { width } = useWindowSize();

  /** convert tags to IOptionItem format */
  const tagOptions: IOptionItem[] = tags.map((tag) => {
    return {
      label: tag.code,
      value: tag.id,
    } as IOptionItem;
  });

  return (
    <styled.Tags className="multi-group">
      <Col>
        <Row>
          <FaTags className="tags-icon" />
          <Select
            isMulti
            width={!!width && width > 500 ? FieldSize.Big : FieldSize.Medium}
            name="tags"
            isDisabled
            options={tagOptions}
            value={
              contentTags.length &&
              tagOptions.filter((option) => contentTags.some((tag) => tag.id === option.value))
            }
            maxMenuHeight={120}
            onChange={(selectedTags) => {}}
          />
        </Row>
      </Col>
    </styled.Tags>
  );
};
