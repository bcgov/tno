import { useFormikContext } from 'formik';
import _ from 'lodash';
import { useRef } from 'react';
import { useLookup } from 'store/hooks';

import { IContentForm } from '../interfaces';

export interface IExtractTagsProps {
  setParsedTags?: React.Dispatch<React.SetStateAction<string[]>>;
}

export const useExtractTags = ({ setParsedTags }: IExtractTagsProps) => {
  const { setFieldValue, values } = useFormikContext<IContentForm>();
  const [{ tags }] = useLookup();

  // Save previous parsed tags for comparison
  const previousMatchesRef = useRef<string[]>([]);

  // Use debounced function to delay processing, ensuring keyboard input stops before updating
  const debouncedSetFieldValue = _.debounce((field: string, value: any) => {
    setFieldValue(field, value);
  }, 300);

  return (name: string, text: string) => {
    // First, immediately update the text field
    setFieldValue(name, text);

    if (!setParsedTags) {
      return;
    }

    // Clean text and parse tags
    const noTagString = text.replace(/<\/?p>/g, '');

    // Check for incomplete tags
    const hasIncompleteTagLeft = /\[[^\]]*$/.test(noTagString); // Left bracket without matching right bracket
    const hasIncompleteTagRight = /][^[]*\[/.test(noTagString); // Right bracket followed by left bracket but no pair
    const hasUnpairedBrackets =
      (noTagString.match(/\[/g) || []).length !== (noTagString.match(/\]/g) || []).length;

    // Use regex to find all complete tags
    const regex = new RegExp(/\[([^\]]*)\]/g);
    const matches = noTagString.match(regex) || [];

    // Check for changes since last tag
    const previousMatches = previousMatchesRef.current;
    const matchesChanged =
      previousMatches.length !== matches.length ||
      !matches.every((match) => previousMatches.includes(match));

    // Update previous matches
    previousMatchesRef.current = [...matches];

    // Get available tags (enabled and selected)
    const availableTags = tags.map((t) => t.code.toUpperCase());

    // Parse found tags
    const parsedTags: string[] = [];
    if (matches.length > 0) {
      matches.forEach((match: string) => {
        const tagContent = match.replace(/[[\]]/g, '');
        const tagArray = tagContent.split(/[\s,]+/).filter(Boolean);

        tagArray.forEach((tag: string) => {
          if (availableTags.includes(tag.toUpperCase())) {
            parsedTags.push(tag.toUpperCase());
          }
        });
      });
    }

    // Get current and other field text, safely
    let otherText = '';

    if (name === 'body') {
      otherText = typeof values.summary === 'string' ? values.summary : '';
    } else if (name === 'summary') {
      otherText = typeof values.body === 'string' ? values.body : '';
    }

    // Parse tags from both fields
    const currentFieldMatches = matches;
    const otherFieldMatches = otherText.replace(/<\/?p>/g, '').match(regex) || [];

    // Parse tags from the other field
    const otherFieldTags: string[] = [];
    if (otherFieldMatches.length > 0) {
      otherFieldMatches.forEach((match: string) => {
        const tagContent = match.replace(/[[\]]/g, '');
        const tagArray = tagContent.split(/[\s,]+/).filter(Boolean);

        tagArray.forEach((tag: string) => {
          if (availableTags.includes(tag.toUpperCase())) {
            otherFieldTags.push(tag.toUpperCase());
          }
        });
      });
    }

    // Check if possibly deleting tags
    // 1. There are incomplete tags
    // 2. The number of tags has decreased
    // 3. There are unpaired brackets
    const isDeletingTags =
      hasIncompleteTagLeft ||
      hasIncompleteTagRight ||
      hasUnpairedBrackets ||
      (matchesChanged && previousMatches.length > matches.length);

    // Special case: possibly deleting tags
    if (isDeletingTags) {
      // If current field has no complete tags, but another field has tags, keep the other field's tags
      if (matches.length === 0 && otherFieldTags.length > 0) {
        // Transfer tags from the other field to the current field
        setParsedTags(otherFieldTags);

        // Update tags field, only containing tags extracted from the other field
        const otherFieldTagObjects = tags.filter((tag) =>
          otherFieldTags.includes(tag.code.toUpperCase()),
        );
        debouncedSetFieldValue('tags', otherFieldTagObjects);
        return;
      }

      // If both fields have no tags, clear
      if (matches.length === 0 && otherFieldTags.length === 0) {
        setFieldValue('tags', []);
        setParsedTags([]);
        return;
      }

      // If there are complete tags but fewer - continue below to merge tags from both fields
      if (matchesChanged && previousMatches.length > matches.length) {
        // Continue below
      }
    }

    // Merge tags from both fields
    const allTags: string[] = [];

    // Process current field tags
    if (currentFieldMatches.length > 0) {
      currentFieldMatches.forEach((match: string) => {
        const tagContent = match.replace(/[[\]]/g, '');
        const tagArray = tagContent.split(/[\s,]+/).filter(Boolean);

        tagArray.forEach((tag: string) => {
          if (availableTags.includes(tag.toUpperCase())) {
            allTags.push(tag.toUpperCase());
          }
        });
      });
    }

    // Process tags from the other field
    if (otherFieldMatches.length > 0) {
      otherFieldMatches.forEach((match: string) => {
        const tagContent = match.replace(/[[\]]/g, '');
        const tagArray = tagContent.split(/[\s,]+/).filter(Boolean);

        tagArray.forEach((tag: string) => {
          if (availableTags.includes(tag.toUpperCase())) {
            allTags.push(tag.toUpperCase());
          }
        });
      });
    }

    // Update parent component tags
    setParsedTags(parsedTags);

    // Use debounced update of form tags field to ensure UI has time to respond
    if (allTags.length === 0) {
      // When no tags, clear immediately, no debouncing
      setFieldValue('tags', []);
    } else {
      // Remove duplicates and map to tag objects
      const uniqueTags = allTags.filter((tag, index) => allTags.indexOf(tag) === index);

      const newTags = tags.filter((tag) => uniqueTags.includes(tag.code.toUpperCase()));

      // Use debounced update of tags field to avoid frequent updates
      debouncedSetFieldValue('tags', newTags);
    }
  };
};
