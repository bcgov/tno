import { type IAutomationStepModel } from '../interfaces';

/**
 * The target used when building the step's default prompt. A start/end step that iterates its
 * step filter results behaves like a content step (`{content}` has a value per item).
 */
export const effectivePromptTarget = (step: IAutomationStepModel): IAutomationStepModel['target'] =>
  step.iterateStepFilter && (step.target === 'start' || step.target === 'end')
    ? 'content'
    : step.target;
