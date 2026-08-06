import { type IAutomationStepModel } from '../interfaces';
import { type StepModalMode } from './StepModalMode';

export interface IStepModalState {
  mode: StepModalMode;
  index?: number;
  step: IAutomationStepModel;
}
