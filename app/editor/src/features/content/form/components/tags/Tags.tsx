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

  // Get tag IDs from tag codes
  const getTagIdsByCode = React.useCallback(
    (tagCodes: string[]) => {
      return tags
        .filter((tag) => tagCodes.some((code) => code === tag.code.toUpperCase()))
        .map((tag) => tag.id);
    },
    [tags],
  );

  // Get tag codes from tag IDs
  const getTagCodesByIds = React.useCallback(
    (tagIds: number[]) => {
      return tags
        .filter((tag) => tagIds.includes(tag.id))
        .map((tag) => tag.code.toUpperCase());
    },
    [tags],
  );

  // Update text with tags - only updates the specified field
  const updateTextTags = React.useCallback(
    (field: 'body' | 'summary', tagCodes: string[]) => {
      // Ensure it works even with an empty document
      let currentText = (values[field] as string | undefined) ?? '';

      // Check for trailing newline
      const hasTrailingNewline = currentText?.endsWith('\n') || false;

      // Remove all [...] formatted tags
      let newText = (currentText || '').replace(/\s*\[([^\]]*)\](\s|$)*/g, '').trimEnd();

      // Add new tags (if any)
      if (tagCodes.length > 0) {
        const tagText = `[${tagCodes.join(', ')}]`;

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

      // Update the field
      setFieldValue(field, newText);

      // Force a refresh after text update
      setRefreshCounter((prev) => prev + 1);
    },
    [values, setFieldValue],
  );

  // Check if current field needs tags migration
  const checkTagsMigration = React.useCallback(() => {
    if (!targetField) return false;
    
    // Get tags from the target field's text
    const fieldValue = values[targetField as keyof typeof values] as string | undefined || '';
    const fieldTagCodes = parseTagsFromText(fieldValue);
    
    // Get tags that should be in this field based on the form's tag selection
    const allSelectedTagCodes = getTagCodesByIds(values.tags.map(t => t.id));
    
    // Check if all selected tags are in the current field
    const needsMigration = !allSelectedTagCodes.every((tag) => fieldTagCodes.includes(tag));
    return needsMigration;
  }, [values, targetField, parseTagsFromText, getTagCodesByIds]);

  // Update migration button visibility when content changes
  React.useEffect(() => {
    if (targetField) {
      const shouldShow = checkTagsMigration();
      setShowMigrateButton(shouldShow);
    }
  }, [values, targetField, checkTagsMigration]);

  // Handle tag selection - this is where we apply field-specific logic
  const addTags = React.useCallback(
    (selectedTagOptions: any) => {
      // Always update the main form tags list with all selected tags
      const allSelectedTags = tags
        .filter((tag) => selectedTagOptions.some((t: IOptionItem) => t.value === tag.id))
        .map((tag) => tag);
      
      setFieldValue('tags', allSelectedTags);
      setSelectedOptions(selectedTagOptions);
      
      // Force a refresh
      setRefreshCounter((prev) => prev + 1);
      
      // Only proceed with automatic text updates if enabled and target field is specified
      if (enableAutoTagText && targetField) {
        // Get the current tags from the target field
        const fieldValue = values[targetField] as string | undefined || '';
        const fieldTagCodes = parseTagsFromText(fieldValue);
        
        // Get the other field's tags (to maintain them separately)
        const otherField = targetField === 'body' ? 'summary' : 'body';
        const otherFieldValue = values[otherField] as string | undefined || '';
        const otherFieldTagCodes = parseTagsFromText(otherFieldValue);
        
        // Get all selected tag codes
        const allSelectedTagCodes = getTagCodesByIds(
          selectedTagOptions.map((option: IOptionItem) => option.value)
        );
        
        // Add newly selected tags to the field's existing tags
        // Remove tags that are no longer selected
        const updatedFieldTags = Array.from(new Set([
          ...fieldTagCodes.filter(code => 
            // Keep tags that are still selected
            allSelectedTagCodes.includes(code)
          ),
          ...allSelectedTagCodes.filter(code => 
            // Add new tags not previously in this field
            // AND not already in the other field (this is the key change)
            !fieldTagCodes.includes(code) && !otherFieldTagCodes.includes(code)
          )
        ]));
        
        // Update the target field with its specific tags
        updateTextTags(targetField, updatedFieldTags);
      }
    },
    [setFieldValue, tags, enableAutoTagText, targetField, updateTextTags, values, parseTagsFromText, getTagCodesByIds],
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
                // Get all selected tag codes
                const allSelectedTagCodes = values.tags.map((tag) => tag.code.toUpperCase());
                
                // Get existing field tag codes
                const fieldValue = values[targetField] as string | undefined || '';
                const fieldTagCodes = parseTagsFromText(fieldValue);
                
                // Get new tags to add to this field
                const tagsToAdd = allSelectedTagCodes.filter(code => !fieldTagCodes.includes(code));
                const updatedFieldTags = [...fieldTagCodes, ...tagsToAdd];
                
                // Update the field
                updateTextTags(targetField, updatedFieldTags);
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
