import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import _ from 'lodash';
import React from 'react';
import { FaArrowRight, FaListAlt } from 'react-icons/fa';
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
  const [showMigrateButton, setShowMigrateButton] = React.useState(false);
  const [tagOptions, setTagOptions] = React.useState<IOptionItem[]>([]);
  const [selectedOptions, setSelectedOptions] = React.useState<IOptionItem[]>([]);

  // A state to force refresh of the component
  const [refreshCounter, setRefreshCounter] = React.useState(0);

  // Force a refresh of the component every second if there are pending tag changes
  React.useEffect(() => {
    // Create a comparison between current tags and selected options
    const currentTagIds = values.tags
      .map((t) => t.id)
      .sort()
      .join(',');
    const selectedTagIds = selectedOptions
      .map((o) => o.value)
      .sort()
      .join(',');

    // If there's a mismatch, we need to poll for changes
    if (currentTagIds !== selectedTagIds) {
      const timer = setInterval(() => {
        setRefreshCounter((prev) => prev + 1);
      }, 500); // Poll every 500ms

      return () => clearInterval(timer);
    }
  }, [values.tags, selectedOptions]);

  // Update tag options when tags list changes
  React.useEffect(() => {
    const newTagOptions = tags
      .filter((tag) => tag.isEnabled || values.tags.some((t) => t.id === tag.id))
      .map((tag) => {
        return {
          label: tag.code,
          value: tag.id,
          isDisabled: !tag.isEnabled,
        } as IOptionItem;
      });

    setTagOptions(newTagOptions);

    // Update selected tags when options change
    updateSelectedTags(newTagOptions);

    // Only update options if the tags list is updated.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tags]);

  // Update selected tags when form tags change
  React.useEffect(() => {
    updateSelectedTags(tagOptions);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values.tags, refreshCounter]);

  // Update selected tags
  const updateSelectedTags = React.useCallback(
    (options: IOptionItem[]) => {
      const selected = options.filter((option) =>
        values.tags.some((tag) => tag.id === option.value),
      );
      setSelectedOptions(selected);
    },
    [values.tags],
  );

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

      // Update the current field
      setFieldValue(field, newText);

      // Clear tags from the other field
      const otherField = field === 'body' ? 'summary' : 'body';
      const otherText = (values[otherField as keyof typeof values] as string | undefined) ?? '';
      if (otherText) {
        // Remove all [...] formatted tags from the other field
        const cleanedText = otherText.replace(/\s*\[([^\]]*)\](\s|$)*/g, '').trimEnd();
        setFieldValue(otherField, cleanedText);
      }

      // Force a refresh after text update
      setRefreshCounter((prev) => prev + 1);
    },
    [values, setFieldValue],
  );
  // Parse tags from text content
  const parseTagsFromText = React.useCallback((text: string): string[] => {
    const tagPattern = /\[([^\]]+)\]/g;
    const matches = text.match(tagPattern);
    if (!matches) return [];

    const result = matches
      .map((match) =>
        match
          .slice(1, -1)
          .split(',')
          .map((tag) => tag.trim()),
      )
      .flat()
      .map((tag) => tag.toUpperCase());
    return result;
  }, []);

  // Check if current field needs tags migration
  const checkTagsMigration = React.useCallback(() => {
    if (!targetField) return false;
    const fieldValue = values[targetField as keyof typeof values];
    const selectedTags = values.tags.map((tag) => tag.code.toUpperCase());

    // If there are selected tags but no field value, we need migration
    if (!fieldValue && selectedTags.length > 0) return true;

    const currentFieldTags = fieldValue ? parseTagsFromText(fieldValue as string) : [];

    // Check if all selected tags are in the current field
    const needsMigration = !selectedTags.every((tag) => currentFieldTags.includes(tag));
    return needsMigration;
  }, [values, targetField, parseTagsFromText]);

  // Update migration button visibility when content changes
  React.useEffect(() => {
    if (targetField) {
      const shouldShow = checkTagsMigration();
      setShowMigrateButton(shouldShow);
    }
  }, [values, targetField, checkTagsMigration]);

  // Handle tag selection
  const addTags = React.useCallback(
    (selectedTags: any) => {
      const newTags = tags
        .filter((tag) => selectedTags.some((t: IOptionItem) => t.value === tag.id))
        .map((tag) => tag);

      // Update form's tag list
      setFieldValue('tags', newTags);

      // Update selected tags
      setSelectedOptions(selectedTags);

      // Force a refresh
      setRefreshCounter((prev) => prev + 1);

      // Automatically handle tag text
      if (enableAutoTagText) {
        const selectedTagCodes = newTags.map((tag) => tag.code.toUpperCase());

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
            value={selectedOptions}
            onChange={(selectedTags) => addTags(selectedTags)}
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
          {showMigrateButton && targetField && (
            <Button
              tooltip={`Migrate tags to ${targetField}`}
              onClick={() => {
                const selectedTagCodes = values.tags.map((tag) => tag.code.toUpperCase());
                updateTextTags(targetField, selectedTagCodes);
                setShowMigrateButton(false);
              }}
            >
              <FaArrowRight />
            </Button>
          )}
        </Row>
      </Col>
    </styled.Tags>
  );
};
