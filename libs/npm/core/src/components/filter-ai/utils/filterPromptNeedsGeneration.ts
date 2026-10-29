import { htmlToPromptText } from './htmlToPromptText';

/** Whether a non-empty prompt differs from the one used to generate the applied settings. */
export const filterPromptNeedsGeneration = (prompt?: string, generatedPrompt?: string): boolean => {
  const text = htmlToPromptText(prompt).trim();
  return !!text && text !== htmlToPromptText(generatedPrompt).trim();
};
