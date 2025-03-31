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
      .flat();
    return result;
  }, []);

  // Parse tags with original format (case preservation)
  const parseTagsWithOriginalFormat = React.useCallback(
    (text: string): { tag: string; original: string }[] => {
      const tagPattern = /\[([^\]]+)\]/g;
      const matches = text.match(tagPattern);
      if (!matches) return [];

      const result = matches
        .map((match) =>
          match
            .slice(1, -1)
            .split(',')
            .map((tag) => tag.trim())
            .map((tag) => ({
              tag: tag.toUpperCase(),
              original: tag,
            })),
        )
        .flat();
      return result;
    },
    [],
  );

  // Get tag codes from tag IDs - this will only find codes for predefined tags
  const getTagCodesByIds = React.useCallback(
    (tagIds: number[]) => {
      return tags.filter((tag) => tagIds.includes(tag.id)).map((tag) => tag.code);
    },
    [tags],
  );

  // Update text with tags - only updates the specified field
  const updateTextTags = React.useCallback(
    (
      field: 'body' | 'summary',
      tagCodes: string[],
      originalFormats: Record<string, string> = {},
    ) => {
      // Ensure it works even with an empty document
      let currentText = (values[field] as string | undefined) ?? '';

      // Check for trailing newline
      const hasTrailingNewline = currentText?.endsWith('\n') || false;

      // Remove all [...] formatted tags
      let newText = (currentText || '').replace(/\s*\[([^\]]*)\](\s|$)*/g, '').trimEnd();

      // Add new tags (if any)
      if (tagCodes.length > 0) {
        // Format each tag using its original format if available
        const formattedTags = tagCodes.map((code) => originalFormats[code.toUpperCase()] || code);

        const tagText = `[${formattedTags.join(', ')}]`;

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
        // Get the current tags from the target field with original format
        const fieldValue = (values[targetField] as string | undefined) || '';
        const fieldTagsWithFormat = parseTagsWithOriginalFormat(fieldValue);
        const fieldTagCodes = fieldTagsWithFormat.map((t) => t.tag);

        // Create a mapping of uppercase tags to their original format
        const originalFormatMap: Record<string, string> = {};
        fieldTagsWithFormat.forEach(({ tag, original }) => {
          originalFormatMap[tag.toUpperCase()] = original;
        });

        // Get the other field's tags (to maintain them separately)
        const otherField = targetField === 'body' ? 'summary' : 'body';
        const otherFieldValue = (values[otherField] as string | undefined) || '';
        const otherFieldTagCodes = parseTagsFromText(otherFieldValue).map((t) => t.toUpperCase());

        // Get all selected tag codes
        const allSelectedTagCodes = getTagCodesByIds(
          selectedTagOptions.map((option: IOptionItem) => option.value),
        );

        // Identify custom tags - tags that exist in the field but are not in the predefined list
        const customFieldTags = fieldTagCodes.filter(
          (code) => !tags.some((tag) => tag.code.toUpperCase() === code.toUpperCase()),
        );

        // Add newly selected tags to the field's existing tags
        // Remove tags that are no longer selected, but keep custom tags
        const updatedFieldTags = Array.from(
          new Set([
            // Keep custom tags
            ...customFieldTags,
            // Keep tags that are still selected
            ...fieldTagCodes.filter(
              (code) =>
                allSelectedTagCodes.map((c) => c.toUpperCase()).includes(code.toUpperCase()) ||
                // Keep codes that are in current field but not found in predefined tags list
                !tags.some((tag) => tag.code.toUpperCase() === code.toUpperCase()),
            ),
            // Add new tags not previously in this field
            ...allSelectedTagCodes.filter(
              (code) =>
                // Add new tags not previously in this field
                // AND not already in the other field
                !fieldTagCodes.map((c) => c.toUpperCase()).includes(code.toUpperCase()) &&
                !otherFieldTagCodes.includes(code.toUpperCase()),
            ),
          ]),
        );

        // For newly added tags from UI, add them to the original format map
        allSelectedTagCodes.forEach((code) => {
          if (!originalFormatMap[code.toUpperCase()]) {
            originalFormatMap[code.toUpperCase()] = code;
          }
        });

        // Update the target field with its specific tags plus custom tags
        updateTextTags(targetField, updatedFieldTags, originalFormatMap);
      }
    },
    [
      setFieldValue,
      tags,
      enableAutoTagText,
      targetField,
      updateTextTags,
      values,
      parseTagsFromText,
      parseTagsWithOriginalFormat,
      getTagCodesByIds,
    ],
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
        </Row>
      </Col>
    </styled.Tags>
  );
};
