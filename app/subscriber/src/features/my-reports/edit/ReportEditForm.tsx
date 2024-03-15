import { useNavigate } from 'react-router-dom';
import { Show } from 'tno-core';

import { IReportInstanceContentForm } from '../interfaces';
import {
  ReportContentMenuOption,
  ReportMainMenuOption,
  ReportSettingsMenuOption,
} from './constants';
import { ReportEditContentForm, ReportEditSortForm, ReportEditSummaryForm } from './content';
import { ReportPreview } from './preview';
import { ReportEditActions } from './ReportEditActions';
import { useReportEditContext } from './ReportEditContext';
import { ReportEditMenu } from './ReportEditMenu';
import {
  ReportEditDataSourcesForm,
  ReportEditPreferencesForm,
  ReportEditSendForm,
  ReportEditTemplateForm,
} from './settings';
import * as styled from './styled';

export interface IReportEditFormProps {
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
  /** Event fires when the content headline is clicked. */
  onContentClick?: (content?: IReportInstanceContentForm, action?: 'previous' | 'next') => void;
}

/**
 * Provides a component which displays the correct form based on the active menu.
 * @returns Component.
 */
export const ReportEditForm = ({ disabled }: IReportEditFormProps) => {
  const navigate = useNavigate();
  const { values, active, activeRow, setActiveRow, handleNavigate } = useReportEditContext();

  const instance = values.instances.length ? values.instances[0] : undefined;

  return (
    <styled.ReportEditForm className="report-edit-form">
      <ReportEditMenu
        onChange={(path) => {
          setActiveRow?.(undefined);
          navigate(path);
        }}
      />
      {/* Settings Menu */}
      <Show visible={active === ReportMainMenuOption.Settings}>
        <ReportEditTemplateForm />
      </Show>
      <Show visible={active === ReportSettingsMenuOption.DataSources}>
        <ReportEditDataSourcesForm />
      </Show>
      <Show visible={active === ReportSettingsMenuOption.Preferences}>
        <ReportEditPreferencesForm />
      </Show>
      <Show visible={active === ReportSettingsMenuOption.Send}>
        <ReportEditSendForm />
      </Show>
      {/* Content Menu */}
      <Show visible={active === ReportMainMenuOption.Content}>
        <ReportEditContentForm
          disabled={disabled}
          activeRow={activeRow}
          showAdd={!activeRow}
          onContentClick={(content, action) => {
            if (action) {
              handleNavigate(instance, action);
            } else setActiveRow(content);
          }}
        />
      </Show>
      <Show visible={active === ReportContentMenuOption.Sort}>
        <ReportEditSortForm />
      </Show>
      <Show visible={active === ReportContentMenuOption.Summary}>
        <ReportEditSummaryForm disabled={disabled} />
      </Show>
      {/* Preview Menu */}
      <Show visible={active === ReportMainMenuOption.Preview}>
        <ReportPreview />
      </Show>
      <ReportEditActions />
    </styled.ReportEditForm>
  );
};
