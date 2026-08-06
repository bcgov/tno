import { type IAutomationStepModel } from '../interfaces';

/** Whether the step's filter runs as a separate enrichment query (results injected into the prompt). */
export const hasEnrichmentFilter = (step: IAutomationStepModel): boolean =>
  !!step.filterId && !step.applyToAutomationFilter && !step.iterateStepFilter;
