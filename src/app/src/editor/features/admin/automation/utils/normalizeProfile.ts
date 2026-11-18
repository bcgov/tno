import { defaultAutomationProfile } from '../constants/defaultAutomationProfile';
import { type IAutomationProfileModel } from '../interfaces';

export const normalizeProfile = (profile?: IAutomationProfileModel): IAutomationProfileModel => {
  if (!profile) return defaultAutomationProfile;
  return {
    ...defaultAutomationProfile,
    ...profile,
    schedules: Array.isArray(profile.schedules) ? profile.schedules : [],
  };
};
