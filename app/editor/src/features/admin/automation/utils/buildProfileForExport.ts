import { type IAutomationProfileModel } from '../interfaces';
import { buildProfileForSave } from './buildProfileForSave';

// Build the JSON payload for exporting a profile. Primary keys (profile/step/action/schedule ids)
// are omitted - they are meaningless outside this database - and dedupe prior-action links are
// dropped because they reference action ids. The result is imported into the form and saved as a
// new profile, which assigns fresh ids.
export const buildProfileForExport = (values: IAutomationProfileModel) => {
  const saved = buildProfileForSave(values);
  return {
    name: saved.name,
    description: saved.description,
    isEnabled: saved.isEnabled,
    schemaVersion: saved.schemaVersion,
    filterId: saved.filterId,
    llmId: saved.llmId,
    schedules: (saved.schedules ?? []).map((schedule) => ({
      name: schedule.name,
      isEnabled: schedule.isEnabled,
      startAt: schedule.startAt,
      runOn: schedule.runOn,
      runOnWeekDays: schedule.runOnWeekDays,
    })),
    steps: (saved.steps ?? []).map((step) => ({
      name: step.name,
      description: step.description,
      prompt: step.prompt,
      target: step.target,
      filterId: step.filterId,
      applyToAutomationFilter: step.applyToAutomationFilter,
      iterateStepFilter: step.iterateStepFilter,
      llmId: step.llmId,
      sendSeparatePrompts: step.sendSeparatePrompts,
      useChatCompletions: step.useChatCompletions,
      priority: step.priority,
      isEnabled: step.isEnabled,
      actions: (step.actions ?? []).map((action) => ({
        name: action.name,
        prompt: action.prompt,
        actionType: action.actionType,
        maxCalls: action.maxCalls,
        confirmationStatement: action.confirmationStatement,
        contentField: action.contentField,
        contentActionId: action.contentActionId,
        reportId: action.reportId,
        notificationId: action.notificationId,
        objective: action.objective,
        autoExecute: action.autoExecute,
        abortIfNoConfirmation: action.abortIfNoConfirmation,
        worksOn: action.worksOn,
        createIdentifier: action.createIdentifier,
        createClone: action.createClone,
        // Extract Data grid / Create Content mapping configuration.
        settings: action.settings ?? {},
        llmId: action.llmId,
        isEnabled: action.isEnabled,
      })),
    })),
  };
};
