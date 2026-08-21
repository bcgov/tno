import { type IAutomationProfileModel } from '../interfaces';

export const buildProfileForSave = (values: IAutomationProfileModel): IAutomationProfileModel => ({
  id: values.id,
  name: values.name,
  description: values.description,
  isEnabled: values.isEnabled,
  schemaVersion: values.schemaVersion,
  definition: values.definition ?? null,
  llmId: values.llmId,
  schedules: (values.schedules ?? []).map((schedule) => ({
    id: schedule.id ?? 0,
    name: schedule.name,
    isEnabled: schedule.isEnabled,
    startAt: schedule.startAt,
    runOn: schedule.runOn ?? null,
    runOnWeekDays: schedule.runOnWeekDays ?? [],
  })),
});
