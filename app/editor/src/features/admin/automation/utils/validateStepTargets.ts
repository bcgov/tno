import { type IAutomationProfileModel } from '../interfaces';

/**
 * Validate that each step target is consistent with the profile filter configuration.
 * Returns an error message when invalid, otherwise undefined.
 */
export const validateStepTargets = (values: IAutomationProfileModel): string | undefined => {
  if (!values.filterId) {
    const invalid = values.steps.filter((step) => step.target !== 'none');
    if (invalid.length)
      return `When the profile does not include a filter the step target must be 'None'. Fix the following step(s): ${invalid
        .map((step) => step.name)
        .join(', ')}.`;
  } else {
    const invalid = values.steps.filter((step) => step.target === 'none');
    if (invalid.length)
      return `When the profile includes a filter each step target must be 'Content', 'Start', or 'End'. Fix the following step(s): ${invalid
        .map((step) => step.name)
        .join(', ')}.`;
  }
  return undefined;
};
