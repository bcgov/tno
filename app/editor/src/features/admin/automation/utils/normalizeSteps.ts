import { type IAutomationStepModel } from '../interfaces';
import { cloneStep } from './cloneStep';

export const normalizeSteps = (steps: IAutomationStepModel[]): IAutomationStepModel[] =>
  steps.map((step, index) => ({
    ...cloneStep(step),
    priority: index,
  }));
