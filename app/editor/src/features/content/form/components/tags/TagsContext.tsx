import { useFormikContext } from 'formik';
import _ from 'lodash';
import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { useLookup } from 'store/hooks';
import { IOptionItem } from 'tno-core';

import { useTagManagement } from './hooks/useTagManagement';
import { IContentForm, IContentTag, ITagsContextState, ITagsProviderProps, Tag } from './types';

// Create context
const TagsContext = createContext<ITagsContextState | undefined>(undefined);

/**
 * Custom hook to use tags context
 * @returns The tags context
 */
export const useTagsContext = () => {
  const context = useContext(TagsContext);
  if (context === undefined) {
    throw new Error('useTagsContext must be used within a TagsProvider');
  }
  return context;
};

/**
 * Tags provider component
 * Manages tag selection and synchronization with text fields
 */
export const TagsProvider: React.FC<ITagsProviderProps> = ({
  children,
  defaultTags = [],
  targetField = 'body',
  enableAutoTagText = true,
  values: externalValues,
  setFieldValue: externalSetFieldValue,
  tags: externalTags,
}) => {
  // If external values are not provided, use Formik and useLookup
  const formikContext = useFormikContext<IContentForm>();
  const lookupContext = useLookup();

  const values = externalValues || formikContext.values;
  const setFieldValue = externalSetFieldValue || formikContext.setFieldValue;
  const tags = externalTags || lookupContext[0].tags;

  // UI state
  const [showList, setShowList] = useState(false);

  // Store UI tag state, ensuring text input doesn't remove selected tags
  const [uiSelectedTags, setUiSelectedTags] = useState<IContentTag[]>([]);

  // Get tag management utilities
  const { extractTagsFromFields, processTagSelectionChanges } = useTagManagement({
    values,
    tags,
    setFieldValue,
  });

  // Use useMemo to derive tag options, not as state
  const tagOptions = useMemo(() => {
    return tags
      .filter((tag: Tag) => tag.isEnabled || values.tags.some((t: IContentTag) => t.id === tag.id))
      .map(
        (tag: Tag) =>
          ({
            label: tag.code,
            value: tag.id,
            isDisabled: !tag.isEnabled,
          } as IOptionItem),
      );
  }, [tags, values.tags]);

  // Use useMemo to derive selected options, not as state
  const selectedOptions = useMemo(() => {
    return tagOptions.filter((option) =>
      values.tags.some((tag: IContentTag) => tag.id === option.value),
    );
  }, [tagOptions, values.tags]);

  // Initialize default tags
  useEffect(() => {
    const initTags = tags.filter((tag: Tag) =>
      defaultTags.some((code) => code === tag.code.toUpperCase()),
    );

    // Convert Tag objects to IContentTag objects
    const contentTags = initTags.map((tag) => ({ id: tag.id, name: tag.name }));

    // Combine with existing tags
    const newTags = _.uniqBy([...values.tags, ...contentTags], 'id');

    setFieldValue('tags', newTags);
    setUiSelectedTags(newTags);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [defaultTags]);

  // Scroll tag list to view
  useEffect(() => {
    const tagList = document.getElementById('tag-list');
    if (tagList) tagList.scrollIntoView({ behavior: 'smooth' });
  }, [showList]);

  // Update form tags based on text fields
  useEffect(() => {
    // Get text tags from both fields
    const extractedTags = extractTagsFromFields();

    // Find new tags that are in text but not in UI
    const newTagsFromText = extractedTags.filter(
      (textTag) => !uiSelectedTags.some((uiTag) => uiTag.id === textTag.id),
    );

    // Only update UI when new tags are found
    if (newTagsFromText.length > 0) {
      // Combine existing UI tags with new tags from text
      const updatedTags = _.uniqBy([...uiSelectedTags, ...newTagsFromText], 'id');

      // Update form tags and UI state
      setFieldValue('tags', updatedTags);
      setUiSelectedTags(updatedTags);
    }
  }, [values.body, values.summary, extractTagsFromFields, uiSelectedTags, setFieldValue]);

  // Handle tag selection changes
  const addTags = useCallback(
    (selectedTagOptions: IOptionItem[]) => {
      // Get previously selected option IDs
      const previousSelectedIds = selectedOptions.map((option) => option.value as number);

      // Get newly selected option IDs
      const currentSelectedIds = selectedTagOptions.map((option) => option.value as number);

      // Update main form tag list - just update with the selected tags
      const allSelectedTags = tags
        .filter((tag: Tag) => selectedTagOptions.some((t: IOptionItem) => t.value === tag.id))
        .map((tag: Tag) => ({ id: tag.id, name: tag.name }));

      // Update form values with selected tags and update UI state
      setFieldValue('tags', allSelectedTags);
      setUiSelectedTags(allSelectedTags);

      // Process tag changes for text fields
      processTagSelectionChanges(
        currentSelectedIds,
        previousSelectedIds,
        targetField,
        enableAutoTagText,
      );
    },
    [
      tags,
      setFieldValue,
      enableAutoTagText,
      targetField,
      selectedOptions,
      processTagSelectionChanges,
    ],
  );

  // Values for context
  const contextValue = useMemo(
    () => ({
      tagOptions,
      selectedOptions,
      showList,
      setShowList,
      addTags,
    }),
    [tagOptions, selectedOptions, showList, addTags],
  );

  return <TagsContext.Provider value={contextValue}>{children}</TagsContext.Provider>;
};
