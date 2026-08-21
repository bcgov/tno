import { type IAutomationProfileModel } from '../interfaces';
import { buildProfileForSave } from './buildProfileForSave';

// Build the JSON payload for exporting a profile. Primary keys (profile/schedule ids) are
// omitted - they are meaningless outside this database. The result is imported into the form
// and saved as a new profile, which assigns fresh ids.
export const buildProfileForExport = (values: IAutomationProfileModel) => {
  const saved = buildProfileForSave(values);
  return {
    name: saved.name,
    description: saved.description,
    isEnabled: saved.isEnabled,
    schemaVersion: saved.schemaVersion,
    // The definition document travels whole; filter/LLM ids inside it are remapped on import.
    definition: saved.definition ?? null,
    llmId: saved.llmId,
    schedules: (saved.schedules ?? []).map((schedule) => ({
      name: schedule.name,
      isEnabled: schedule.isEnabled,
      startAt: schedule.startAt,
      runOn: schedule.runOn,
      runOnWeekDays: schedule.runOnWeekDays,
    })),
  };
};
