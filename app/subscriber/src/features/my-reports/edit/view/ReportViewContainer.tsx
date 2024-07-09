import { ReportMainMenuOption } from '../constants';
import { useReportEditContext } from '../ReportEditContext';
import { ReportHistoryView } from './ReportHistoryView';
import { ReportView } from './ReportView';

export const ReportContainer = () => {
  const { active } = useReportEditContext();

  if (!active) return null;

  return (
    <>{active?.startsWith(ReportMainMenuOption.View) ? <ReportView /> : <ReportHistoryView />}</>
  );
};
