import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import _ from 'lodash';
import React from 'react';
import { FaListAlt } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { Button, Col, FieldSize, IOptionItem, Row, Select } from 'tno-core';

import { DraggableTagList } from './DraggableTagList';
import * as styled from './styled';

export interface ITagsProps {
  defaultTags?: string[];
  /** the field to update with tags */
  targetField?: 'body' | 'summary';
  /** whether to enable automatic tag text updates, default is true */
  enableAutoTagText?: boolean;
}

/**
 * The component that renders tags for a given text field
 * @returns the Tags component
 */
export const Tags: React.FC<ITagsProps> = ({
  defaultTags = [],
  targetField,
  enableAutoTagText = true,
}) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [{ tags }] = useLookup();

  const [showList, setShowList] = React.useState(false);
  const [lastProcessedTags, setLastProcessedTags] = React.useState<string[]>([]);
  const [tagOptions, setTagOptions] = React.useState(
    tags
      .filter((tag) => tag.isEnabled || values.tags.some((t) => t.id === tag.id))
      .map((tag) => {
        return {
          label: tag.code,
          value: tag.id,
          isDisabled: !tag.isEnabled,
        } as IOptionItem;
      }),
  );

  React.useEffect(() => {
    setTagOptions(
      tags
        .filter((tag) => tag.isEnabled || values.tags.some((t) => t.id === tag.id))
        .map((tag) => {
          return {
            label: tag.code,
            value: tag.id,
            isDisabled: !tag.isEnabled,
          } as IOptionItem;
        }),
    );
    // Only update options if the tags list is updated.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tags]);

  /** ensure table is in view depending on where user has scrolled to. */
  React.useEffect(() => {
    if (document.getElementById('tag-list')) {
      document.getElementById('tag-list')?.scrollIntoView({ behavior: 'smooth' });
    }
  }, [showList]);

  React.useEffect(() => {
    const initTags = tags.filter((tag) =>
      defaultTags.some((code) => code === tag.code.toUpperCase()),
    );
    const newTags = _.uniqBy(values.tags.concat(initTags), (tag) => tag.code);
    setFieldValue('tags', newTags);
    // Only update if the default values have changed.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [defaultTags]);

  // Update text with tags
  const updateTextTags = React.useCallback(
    (field: 'body' | 'summary', selectedTagCodes: string[]) => {
      // Ensure it works even with an empty document
      let currentText = (values[field] as string | undefined) ?? '';

      // Remove all existing tags
      const hasTrailingNewline = currentText?.endsWith('\n') || false;

      // Remove all [...] formatted tags
      let newText = (currentText || '').replace(/\s*\[([^\]]*)\](\s|$)*/g, '').trimEnd();

      // Add new tags (if any)
      if (selectedTagCodes.length > 0) {
        const tagText = `[${selectedTagCodes.join(', ')}]`;

        // Handle HTML content
        if (currentText?.includes('</p>')) {
          // Replace last </p> tag
          newText = newText.replace(/<\/p>\s*$/, '');
          newText = newText ? `${newText} ${tagText}</p>` : `<p>${tagText}</p>`;
        } else {
          // Handle plain text
          newText = newText ? `${newText} ${tagText}` : tagText;
        }
      }

      // Keep original trailing newline
      if (hasTrailingNewline) {
        newText += '\n';
      }

      setFieldValue(field, newText);
    },
    [values, setFieldValue],
  );

  // Handle tag selection
  const addTags = React.useCallback(
    (selectedTags: any) => {
      const newTags = tags
        .filter((tag) => selectedTags.some((t: IOptionItem) => t.value === tag.id))
        .map((tag) => tag);

      // Update form's tag list
      setFieldValue('tags', newTags);

      // Automatically handle tag text
      if (enableAutoTagText) {
        const selectedTagCodes = newTags.map((tag) => tag.code.toUpperCase());
        setLastProcessedTags(selectedTagCodes);

        // Determine which field to update
        let fieldToUpdate = targetField;
        if (!fieldToUpdate) {
          // If no field specified, default to body
          fieldToUpdate = 'body';
        }

        // Always update the field, even if it's empty
        updateTextTags(fieldToUpdate, selectedTagCodes);
      }
    },
    [setFieldValue, tags, enableAutoTagText, targetField, updateTextTags],
  );

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
            value={tagOptions.filter((option) =>
              values.tags.find((tag) => tag.id === option.value),
            )}
            onChange={(selectedTags) => addTags(selectedTags)}
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
