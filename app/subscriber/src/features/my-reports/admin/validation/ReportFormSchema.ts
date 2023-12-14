import { ReportFormSettingsSchema } from './ReportFormSettingsSchema';
import { ReportFormTemplateSchema } from './ReportFormTemplateSchema';

export const ReportFormSchema = ReportFormTemplateSchema.concat(ReportFormSettingsSchema);
