import { defaultAutomationProfile } from '../constants/defaultAutomationProfile';
import {
  type IAutomationLegacyProfileModel,
  type IAutomationProfileModel,
  type IAutomationStepModel,
} from '../interfaces';
import { normalizeSteps } from './normalizeSteps';

export const normalizeProfile = (
  profile?: IAutomationLegacyProfileModel,
): IAutomationProfileModel => {
  if (!profile) return defaultAutomationProfile;
  const legacyProfile = profile as IAutomationLegacyProfileModel & {
    ai?: { llmId?: number };
    rules?: IAutomationStepModel[];
    steps?: IAutomationStepModel[];
    Rules?: IAutomationStepModel[];
    Steps?: IAutomationStepModel[];
    FilterId?: number;
    LLMId?: number;
  };
  const normalizedSteps =
    legacyProfile.steps ?? legacyProfile.Steps ?? legacyProfile.rules ?? legacyProfile.Rules ?? [];
  const rawSteps = Array.isArray(normalizedSteps) ? normalizedSteps : [];

  return {
    ...defaultAutomationProfile,
    ...legacyProfile,
    filterId: legacyProfile.filterId ?? legacyProfile.FilterId,
    llmId: legacyProfile.llmId ?? legacyProfile.LLMId ?? legacyProfile.ai?.llmId,
    schedules: Array.isArray(legacyProfile.schedules) ? legacyProfile.schedules : [],
    steps: normalizeSteps(rawSteps.length ? rawSteps : defaultAutomationProfile.steps),
  };
};
