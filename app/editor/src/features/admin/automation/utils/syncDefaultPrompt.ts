import { buildDefaultContentStepPrompt } from '../constants/defaultAutomationProfile';
import { type IAutomationStepModel } from '../interfaces';
import { effectivePromptTarget } from './effectivePromptTarget';
import { hasEnrichmentFilter } from './hasEnrichmentFilter';

/** Every default step prompt variant; a prompt matching any of these has not been customized. */
const defaultStepPromptVariants = [false, true].flatMap((chat) =>
  [false, true].flatMap((results) =>
    (['content', 'end'] as const).map((target) =>
      buildDefaultContentStepPrompt(results, target, chat),
    ),
  ),
);

/** Re-sync the step prompt to the matching default when it has not been customized. */
export const syncDefaultPrompt = (step: IAutomationStepModel): IAutomationStepModel => {
  const isDefaultPrompt = !step.prompt || defaultStepPromptVariants.includes(step.prompt);
  return isDefaultPrompt
    ? {
        ...step,
        prompt: buildDefaultContentStepPrompt(
          hasEnrichmentFilter(step),
          effectivePromptTarget(step),
          step.useChatCompletions,
        ),
      }
    : step;
};
