import { useNavigate } from 'react-router-dom';
import { Show } from 'tno-core';

import { IReportForm, IReportInstanceContentForm } from '../interfaces';
import {
  ReportContentMenuOption,
  ReportSendMenuOption,
  ReportSettingsMenuOption,
  ReportViewMenuOption,
} from './constants';
import { ReportEditContentForm, ReportEditSortForm, ReportEditSummaryForm } from './content';
import { ReportEditActions } from './ReportEditActions';
import { useReportEditContext } from './ReportEditContext';
import { ReportEditMenu } from './ReportEditMenu';
import {
  ReportEditDataSourcesForm,
  ReportEditDetailsForm,
  ReportEditPreferencesForm,
  ReportEditSendForm,
  ReportEditTemplateForm,
} from './settings';
import * as styled from './styled';
import { ReportSendForm, ReportView } from './view';

export interface IReportEditFormProps {
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
  /** Event fires when the content headline is clicked. */
  onContentClick?: (content?: IReportInstanceContentForm, action?: 'previous' | 'next') => void;
  /** Event to update the original report. */
  updateForm: (values: IReportForm) => void;
}

/**
 * Provides a component which displays the correct form based on the active menu.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportEditForm = ({ disabled, updateForm }: IReportEditFormProps) => {
  const navigate = useNavigate();
  const { values, active, activeRow, setActiveRow, onNavigate } = useReportEditContext();

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
      <Show visible={active === ReportSettingsMenuOption.Info}>
        <ReportEditDetailsForm />
      </Show>
      <Show visible={active === ReportSettingsMenuOption.Sections}>
        <ReportEditTemplateForm />
      </Show>
      <Show visible={active === ReportSettingsMenuOption.DataSources}>
        <ReportEditDataSourcesForm />
      </Show>
      <Show visible={active === ReportSettingsMenuOption.Preferences}>
        <ReportEditPreferencesForm />
      </Show>
      <Show visible={active === ReportSettingsMenuOption.Send}>
        <ReportEditSendForm updateForm={updateForm} />
      </Show>
      {/* Content Menu */}
      <Show visible={active === ReportContentMenuOption.Content}>
        <ReportEditContentForm
          disabled={disabled}
          activeRow={activeRow}
          showAdd={!activeRow}
          onContentClick={(content, action) => {
            if (action) {
              onNavigate(instance, action);
            } else setActiveRow(content);
          }}
        />
      </Show>
      <Show visible={active === ReportContentMenuOption.Sort}>
        <ReportEditSortForm
          disabled={disabled}
          activeRow={activeRow}
          onContentClick={(content, action) => {
            if (action) {
              onNavigate(instance, action);
            } else setActiveRow(content);
          }}
        />
      </Show>
      <Show visible={active === ReportContentMenuOption.Summary}>
        <ReportEditSummaryForm disabled={disabled} />
      </Show>
      {/* Preview Menu */}
      <Show visible={active === ReportViewMenuOption.View}>
        <ReportView />
      </Show>
      {/* Send Menu */}
      <Show visible={active === ReportSendMenuOption.Send}>
        <ReportSendForm />
      </Show>
      <ReportEditActions disabled={disabled} updateForm={updateForm} />
    </styled.ReportEditForm>
  );
};
