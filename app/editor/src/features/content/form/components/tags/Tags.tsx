import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaListAlt } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { Button, Col, FieldSize, IOptionItem, Row, Select } from 'tno-core';

import { DraggableTagList } from './DraggableTagList';
import * as styled from './styled';
import { TagsProvider, useTagsContext } from './TagsContext';

export interface ITagsProps {
  defaultTags?: string[];
  /** the field to update with tags */
  targetField?: 'body' | 'summary';
  /** whether to enable automatic tag text updates, default is true */
  enableAutoTagText?: boolean;
}

/**
 * Internal Tags component that uses the context
 */
const TagsComponent: React.FC = () => {
  const { tagOptions, selectedOptions, showList, setShowList, addTags, refreshCounter } =
    useTagsContext();

  return (
    <styled.Tags className="multi-group">
      <DraggableTagList showList={showList} setShowList={setShowList} />
      <Col>
        <Row>
          <label>Tags</label>
        </Row>
        <Row>
          <Select
            isMulti
            width={FieldSize.Big}
            name="tags"
            options={tagOptions}
            maxMenuHeight={120}
            menuPlacement="top"
            value={selectedOptions}
            onChange={(newValue) => addTags(newValue as IOptionItem[])}
            key={`tag-select-${refreshCounter}`} // Force re-render when refreshCounter changes
          />
          <Button
            tooltip="Show tag list"
            onClick={() => {
              setShowList(!showList);
            }}
          >
            <FaListAlt />
          </Button>
        </Row>
      </Col>
    </styled.Tags>
  );
};

/**
 * The component that renders tags for a given text field
 * @returns the Tags component
 */
export const Tags: React.FC<ITagsProps> = ({
  defaultTags = [],
  targetField = 'body',
  enableAutoTagText = true,
}) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [{ tags }] = useLookup();

  return (
    <TagsProvider
      defaultTags={defaultTags}
      targetField={targetField}
      enableAutoTagText={enableAutoTagText}
      values={values}
      setFieldValue={setFieldValue}
      tags={tags}
    >
      <TagsComponent />
    </TagsProvider>
  );
};
