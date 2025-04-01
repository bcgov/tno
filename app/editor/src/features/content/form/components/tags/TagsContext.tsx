import { useFormikContext } from 'formik';
import _ from 'lodash';
import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { useLookup } from 'store/hooks';
import { IOptionItem } from 'tno-core';

// import interfaces
interface IContentTag {
  id: number;
  name: string;
}

interface IContentForm {
  tags: IContentTag[];
  body?: string;
  summary?: string;
  [key: string]: any;
}

interface Tag {
  id: number;
  code: string;
  name: string;
  isEnabled: boolean;
}

// define context interface
interface ITagsContextState {
  // dropdown menu tag options
  tagOptions: IOptionItem[];
  // selected options in dropdown menu
  selectedOptions: IOptionItem[];
  // show tag list
  showList: boolean;
  // set show tag list
  setShowList: (show: boolean) => void;
  // handle tag selection changes
  addTags: (selectedTagOptions: unknown) => void;
  // handle tag removal
  handleTagRemoval: (removedTagIds: number[]) => void;
  // force refresh counter
  refreshCounter: number;
}

// create context
const TagsContext = createContext<ITagsContextState | undefined>(undefined);

// custom hook to use tags context
export const useTagsContext = () => {
  const context = useContext(TagsContext);
  if (context === undefined) {
    throw new Error('useTagsContext must be used within a TagsProvider');
  }
  return context;
};

// define tags provider props
interface TagsProviderProps {
  children: React.ReactNode;
  defaultTags?: string[];
  targetField?: 'body' | 'summary';
  enableAutoTagText?: boolean;
  // allow external injection of these values (mainly for testing)
  values?: IContentForm;
  setFieldValue?: (field: string, value: any) => void;
  tags?: Tag[];
}

// tags provider component
export const TagsProvider: React.FC<TagsProviderProps> = ({
  children,
  defaultTags = [],
  targetField = 'body',
  enableAutoTagText = true,
  values: externalValues,
  setFieldValue: externalSetFieldValue,
  tags: externalTags,
}) => {
  // if external values are not provided, use Formik and useLookup
  const formikContext = useFormikContext<IContentForm>();
  const lookupContext = useLookup();

  const values = externalValues || formikContext.values;
  const setFieldValue = externalSetFieldValue || formikContext.setFieldValue;
  const tags = externalTags || lookupContext[0].tags;

  const [showList, setShowList] = useState(false);
  const [refreshCounter, setRefreshCounter] = useState(0);

  // use useMemo to derive tag options, not as state
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

  // use useMemo to derive selected options, not as state
  const selectedOptions = useMemo(() => {
    return tagOptions.filter((option) =>
      values.tags.some((tag: IContentTag) => tag.id === option.value),
    );
  }, [tagOptions, values.tags]);

  // initialize default tags
  useEffect(() => {
    const initTags = tags.filter((tag: Tag) =>
      defaultTags.some((code) => code === tag.code.toUpperCase()),
    );
    const newTags = _.uniqBy(values.tags.concat(initTags), (tag: Tag | IContentTag) => tag.id);
    setFieldValue('tags', newTags);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [defaultTags]);

  // scroll tag list to view
  useEffect(() => {
    const tagList = document.getElementById('tag-list');
    if (tagList) tagList.scrollIntoView({ behavior: 'smooth' });
  }, [showList]);

  // parse tags from text
  const parseTagsFromText = useCallback((text: string): string[] => {
    const tagPattern = /\[([^\]]+)\]/g;
    const matches = text.match(tagPattern);
    if (!matches) return [];

    return matches
      .map((match) =>
        match
          .slice(1, -1)
          .split(',')
          .map((tag) => tag.trim()),
      )
      .flat();
  }, []);

  // parse tags with original format
  const parseTagsWithOriginalFormat = useCallback(
    (text: string): { tag: string; original: string }[] => {
      const tagPattern = /\[([^\]]+)\]/g;
      const matches = text.match(tagPattern);
      if (!matches) return [];

      return matches
        .map((match) =>
          match
            .slice(1, -1)
            .split(',')
            .map((tag: string) => tag.trim())
            .map((tag: string) => ({
              tag: tag.toUpperCase(),
              original: tag,
            })),
        )
        .flat();
    },
    [],
  );

  // get tag codes by ids
  const getTagCodesByIds = useCallback(
    (tagIds: number[]) => {
      return tags.filter((tag: Tag) => tagIds.includes(tag.id)).map((tag: Tag) => tag.code);
    },
    [tags],
  );

  // create original format map
  const createOriginalFormatMap = useCallback((parsedTags: { tag: string; original: string }[]) => {
    const map: Record<string, string> = {};
    parsedTags.forEach(({ tag, original }) => {
      map[tag.toUpperCase()] = original;
    });
    return map;
  }, []);

  // update text tags
  const updateTextTags = useCallback(
    (
      field: 'body' | 'summary',
      tagCodes: string[],
      originalFormats: Record<string, string> = {},
    ) => {
      // ensure even empty documents work
      let currentText = (values[field] as string | undefined) ?? '';
      const hasTrailingNewline = currentText?.endsWith('\n') || false;

      // remove all [...] formatted tags
      let newText = (currentText || '').replace(/\s*\[([^\]]*)\](\s|$)*/g, '').trimEnd();

      // add new tags (if any)
      if (tagCodes.length > 0) {
        // use original format to format each tag (if available)
        const formattedTags = tagCodes
          .filter((code) => code && code.trim() !== '') // avoid empty tags
          .map((code) => originalFormats[code.toUpperCase()] || code);

        // only create tag text if there are valid tags
        if (formattedTags.length > 0) {
          const tagText = `[${formattedTags.join(', ')}]`;

          // handle HTML content and plain text
          if (currentText?.includes('</p>')) {
            newText = newText.replace(/<\/p>\s*$/, '');
            newText = newText ? `${newText} ${tagText}</p>` : `<p>${tagText}</p>`;
          } else {
            newText = newText ? `${newText} ${tagText}` : tagText;
          }
        }
      }

      // preserve original trailing newline
      if (hasTrailingNewline) {
        newText += '\n';
      }

      // update field and force refresh
      setFieldValue(field, newText);
      setRefreshCounter((prev) => prev + 1);
    },
    [values, setFieldValue],
  );

  // handle tag removal from text field
  const handleTagRemoval = useCallback(
    (removedTagIds: number[]) => {
      if (removedTagIds.length === 0) return false;

      const removedTagCodes = getTagCodesByIds(removedTagIds);
      const removedTagUpperCases = removedTagCodes.map((code) => code.toUpperCase());

      // process both fields (summary and body)
      ['summary', 'body'].forEach((field) => {
        const fieldValue = (values[field as keyof IContentForm] as string) || '';
        const fieldTagsWithFormat = parseTagsWithOriginalFormat(fieldValue);
        const originalFormatMap = createOriginalFormatMap(fieldTagsWithFormat);

        // preserve tags not in removal list
        const updatedFieldTags = fieldTagsWithFormat
          .filter(({ tag }) => !removedTagUpperCases.includes(tag))
          .map(({ tag }) => originalFormatMap[tag.toUpperCase()] || tag)
          .filter((tag) => tag && tag.trim() !== '');

        // if tags are removed, update field
        if (updatedFieldTags.length !== fieldTagsWithFormat.length) {
          updateTextTags(field as 'body' | 'summary', updatedFieldTags, originalFormatMap);
        }
      });

      return true;
    },
    [
      values,
      parseTagsWithOriginalFormat,
      createOriginalFormatMap,
      getTagCodesByIds,
      updateTextTags,
    ],
  );

  // handle tag addition to target field
  const handleTagAddition = useCallback(
    (selectedTagOptions: IOptionItem[]) => {
      if (!enableAutoTagText || !targetField) return;

      // get current tags from target field, preserve original format
      const fieldValue = (values[targetField] as string | undefined) || '';
      const fieldTagsWithFormat = parseTagsWithOriginalFormat(fieldValue);
      const fieldTagCodes = fieldTagsWithFormat.map((t) => t.tag);
      const originalFormatMap = createOriginalFormatMap(fieldTagsWithFormat);

      // get tags from other field
      const otherField = targetField === 'body' ? 'summary' : 'body';
      const otherFieldValue = (values[otherField] as string | undefined) || '';
      const otherFieldTagCodes = parseTagsFromText(otherFieldValue).map((t) => t.toUpperCase());

      // get all selected tag codes
      const allSelectedTagCodes = getTagCodesByIds(
        selectedTagOptions.map((option: IOptionItem) => option.value as number),
      );

      // identify custom tags - tags that exist in the field but not in the pre-defined list
      const customFieldTags = fieldTagCodes.filter(
        (code) => !tags.some((tag: Tag) => tag.code.toUpperCase() === code.toUpperCase()),
      );

      // build updated field tags
      const updatedFieldTags = Array.from(
        new Set([
          // preserve custom tags
          ...customFieldTags,
          // preserve still selected tags or not in pre-defined list
          ...fieldTagCodes.filter(
            (code) =>
              allSelectedTagCodes.map((c) => c.toUpperCase()).includes(code.toUpperCase()) ||
              !tags.some((tag: Tag) => tag.code.toUpperCase() === code.toUpperCase()),
          ),
          // add new selected tags not in this field or other field (case-insensitive)
          ...allSelectedTagCodes.filter(
            (code) =>
              !fieldTagCodes.map((c) => c.toUpperCase()).includes(code.toUpperCase()) &&
              !otherFieldTagCodes.includes(code.toUpperCase()),
          ),
        ]),
      );

      // add new selected tags to original format map
      allSelectedTagCodes.forEach((code) => {
        if (!originalFormatMap[code.toUpperCase()]) {
          originalFormatMap[code.toUpperCase()] = code;
        }
      });

      // update target field
      updateTextTags(targetField, updatedFieldTags, originalFormatMap);
    },
    [
      enableAutoTagText,
      targetField,
      values,
      tags,
      parseTagsWithOriginalFormat,
      parseTagsFromText,
      createOriginalFormatMap,
      getTagCodesByIds,
      updateTextTags,
    ],
  );

  // handle tag selection changes
  const addTags = useCallback(
    (selectedTagOptions: unknown) => {
      // convert unknown type to IOptionItem[]
      const optionsArray = selectedTagOptions as IOptionItem[];

      // get previous and current selected tag IDs
      const previousSelectedTagIds = selectedOptions.map(
        (option: IOptionItem) => option.value as number,
      );
      const currentSelectedTagIds = optionsArray.map(
        (option: IOptionItem) => option.value as number,
      );

      // find removed tag IDs
      const removedTagIds = previousSelectedTagIds.filter(
        (id) => !currentSelectedTagIds.includes(id),
      );

      // update main form tag list
      const allSelectedTags = tags
        .filter((tag: Tag) => optionsArray.some((t: IOptionItem) => t.value === tag.id))
        .map((tag: Tag) => ({ id: tag.id, name: tag.name }));

      setFieldValue('tags', allSelectedTags);
      setRefreshCounter((prev) => prev + 1);

      // handle tags removed from UI
      const tagsWereRemoved = handleTagRemoval(removedTagIds);

      // if no tags were removed, handle tag addition
      if (!tagsWereRemoved) {
        handleTagAddition(optionsArray);
      }
    },
    [tags, selectedOptions, setFieldValue, handleTagRemoval, handleTagAddition],
  );

  // values for context
  const contextValue = useMemo(
    () => ({
      tagOptions,
      selectedOptions,
      showList,
      setShowList,
      addTags,
      handleTagRemoval,
      refreshCounter,
    }),
    [tagOptions, selectedOptions, showList, addTags, handleTagRemoval, refreshCounter],
  );

  return <TagsContext.Provider value={contextValue}>{children}</TagsContext.Provider>;
};
