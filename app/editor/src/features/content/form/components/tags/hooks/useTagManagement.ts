import { useCallback } from 'react';

import { IContentForm, Tag } from '../types';
import {
  createOriginalFormatMap,
  formatTextWithTags,
  getTagCodesByIds,
  parseTagsWithOriginalFormat,
} from '../utils/tagParsing';

interface UseTagManagementProps {
  values: IContentForm;
  tags: Tag[];
  setFieldValue: (field: string, value: any) => void;
}

/**
 * Custom hook for tag management operations
 */
export const useTagManagement = ({ values, tags, setFieldValue }: UseTagManagementProps) => {
  /**
   * Update text tags in a specific field
   */
  const updateTextTags = useCallback(
    (
      field: 'body' | 'summary',
      tagCodes: string[],
      originalFormats: Record<string, string> = {},
    ) => {
      const currentText = (values[field] as string | undefined) ?? '';
      const updatedText = formatTextWithTags(currentText, tagCodes, originalFormats);
      setFieldValue(field, updatedText);
    },
    [values, setFieldValue],
  );

  /**
   * Extract tags from both body and summary fields
   */
  const extractTagsFromFields = useCallback(() => {
    // Get tags from both fields
    const bodyTags = parseTagsWithOriginalFormat((values.body as string) || '');
    const summaryTags = parseTagsWithOriginalFormat((values.summary as string) || '');

    // Combine tags from both fields
    const allTagCodes = [...bodyTags, ...summaryTags].map((t) => t.tag.toUpperCase());

    // Find matching tag objects
    return tags
      .filter((tag) => allTagCodes.includes(tag.code.toUpperCase()))
      .map((tag) => ({ id: tag.id, name: tag.name }));
  }, [values.body, values.summary, tags]);

  /**
   * Handle adding tags to a specific field
   */
  const handleTagAddition = useCallback(
    (field: 'body' | 'summary', tagIdsToAdd: number[]) => {
      // get current tags from target field, preserve original format
      const fieldValue = (values[field] as string | undefined) || '';
      const fieldTagsWithFormat = parseTagsWithOriginalFormat(fieldValue);
      const fieldTagCodes = fieldTagsWithFormat.map((t) => t.tag);
      const originalFormatMap = createOriginalFormatMap(fieldTagsWithFormat);

      // Get codes for newly added tags
      const newTagCodes = getTagCodesByIds(tags, tagIdsToAdd);

      // identify custom tags - tags that exist in the field but not in the pre-defined list
      const customFieldTags = fieldTagCodes.filter(
        (code) => !tags.some((tag: Tag) => tag.code.toUpperCase() === code.toUpperCase()),
      );

      // Combine existing field tags with newly added tags
      const updatedFieldTags = Array.from(
        new Set([
          // preserve existing field tags
          ...fieldTagCodes,
          // preserve custom tags
          ...customFieldTags,
          // add only newly selected tags
          ...newTagCodes,
        ]),
      );

      // add new selected tags to original format map
      newTagCodes.forEach((code) => {
        if (!originalFormatMap[code.toUpperCase()]) {
          originalFormatMap[code.toUpperCase()] = code;
        }
      });

      // update target field
      updateTextTags(field, updatedFieldTags, originalFormatMap);
    },
    [tags, values, updateTextTags],
  );

  /**
   * Handle removing tags from a specific field
   */
  const handleTagRemoval = useCallback(
    (field: 'body' | 'summary', tagIdsToRemove: number[]) => {
      // get current tags from field, preserve original format
      const fieldValue = (values[field] as string | undefined) || '';
      const fieldTagsWithFormat = parseTagsWithOriginalFormat(fieldValue);
      const fieldTagCodes = fieldTagsWithFormat.map((t) => t.tag);
      const originalFormatMap = createOriginalFormatMap(fieldTagsWithFormat);

      // Get codes for removed tags
      const removedTagCodes = getTagCodesByIds(tags, tagIdsToRemove);
      const removedTagUpperCases = removedTagCodes.map((code) => code.toUpperCase());

      // Filter out removed tags
      const updatedFieldTags = fieldTagCodes.filter(
        (code) => !removedTagUpperCases.includes(code.toUpperCase()),
      );

      // Update target field only if tags were actually removed
      if (updatedFieldTags.length < fieldTagCodes.length) {
        updateTextTags(field, updatedFieldTags, originalFormatMap);
      }
    },
    [tags, values, updateTextTags],
  );

  /**
   * Process tag selection changes
   */
  const processTagSelectionChanges = useCallback(
    (
      currentSelectedIds: number[],
      previousSelectedIds: number[],
      targetField: 'body' | 'summary',
      enableAutoTagText: boolean,
    ) => {
      // Find newly added tags (present in current but not in previous)
      const newlyAddedIds = currentSelectedIds.filter((id) => !previousSelectedIds.includes(id));

      // Find removed tags (present in previous but not in current)
      const removedIds = previousSelectedIds.filter((id) => !currentSelectedIds.includes(id));

      if (enableAutoTagText) {
        // If there are newly added tags, add them to the target field
        if (newlyAddedIds.length > 0 && targetField) {
          handleTagAddition(targetField, newlyAddedIds);
        }

        // If there are removed tags, remove them from BOTH fields
        if (removedIds.length > 0) {
          // Remove tags from body field
          handleTagRemoval('body', removedIds);

          // Remove tags from summary field
          handleTagRemoval('summary', removedIds);
        }
      }
    },
    [handleTagAddition, handleTagRemoval],
  );

  return {
    updateTextTags,
    extractTagsFromFields,
    handleTagAddition,
    handleTagRemoval,
    processTagSelectionChanges,
  };
};
