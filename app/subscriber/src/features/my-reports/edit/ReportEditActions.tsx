import { Action } from 'components/action';
import { Button } from 'components/button';
import { Modal } from 'components/modal';
import React from 'react';
import { FaSave, FaTelegramPlane } from 'react-icons/fa';
import { FaCaretRight, FaFileCirclePlus, FaTrash } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReportInstances } from 'store/hooks';
import { ReportStatusName, Show, useModal } from 'tno-core';

import { IReportForm } from '../interfaces';
import {
  ReportContentMenuOption,
  ReportMainMenuOption,
  ReportSettingsMenuOption,
  ReportViewMenuOption,
} from './constants';
import { useReportEditContext } from './ReportEditContext';
import { ReportExporter } from './settings/ReportExporter';
import * as styled from './styled';
import { ReportSubscriberExporter } from './view/ReportSubscriberExporter';

export interface IReportEditActionsProps {
  /** Control which buttons are enabled. */
  disabled?: boolean;
  /** Event to update the original report. */
  updateForm: (values: IReportForm) => void;
}

/**
 * Provides component that provides action buttons for the report administration (i.e. Cancel, Save, Start next report).
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportEditActions = ({ disabled, updateForm }: IReportEditActionsProps) => {
  const {
    values,
    isSubmitting,
    submitForm,
    onGenerate,
    activeRow,
    setFieldValue,
    setValues,
    active,
  } = useReportEditContext();
  const navigate = useNavigate();
  const { toggle: toggleRemove, isShowing: isShowingRemove } = useModal();
  const { toggle: toggleSend, isShowing: isShowingSend } = useModal();
  const [{ publishReportInstance }] = useReportInstances();

  const instance = values.instances.length ? values.instances[0] : undefined;

  const handleRemoveContent = React.useCallback(() => {
    setValues({
      ...values,
      instances: values.instances.map((i) =>
        i.id === instance?.id ? { ...instance, content: [] } : i,
      ),
    });
    submitForm();
  }, [instance, setValues, submitForm, values]);

  const handlePublish = React.useCallback(
    async (id: number) => {
      try {
        const updatedInstance = await publishReportInstance(id);
        setFieldValue(
          'instances',
          values.instances.map((i) =>
            i.id === id ? { ...updatedInstance, content: instance?.content } : i,
          ),
        );
        toast.success('Report has been submitted.');
      } catch {}
    },
    [instance?.content, publishReportInstance, setFieldValue, values.instances],
  );

  return (
    <styled.ReportEditActions className="report-edit-actions">
      <Show visible={active?.startsWith(ReportMainMenuOption.Send)}>
        <ReportSubscriberExporter />

        <Button
          disabled={isSubmitting || !instance || instance?.status === ReportStatusName.Submitted}
          onClick={() => toggleSend()}
        >
          Send to subscribers
          <FaTelegramPlane />
        </Button>
      </Show>
      <Show visible={active?.startsWith(ReportMainMenuOption.Settings)}>
        <ReportExporter />
      </Show>
      <Show
        visible={!!instance?.content.length && active?.startsWith(ReportMainMenuOption.Content)}
      >
        <Action icon={<FaTrash />} label="Remove all stories" onClick={() => toggleRemove()} />
      </Show>

      <Show
        visible={
          !activeRow &&
          (active?.startsWith(ReportMainMenuOption.Settings) ||
            active?.startsWith(ReportMainMenuOption.Content))
        }
      >
        <Button variant="secondary" onClick={() => navigate('/reports')}>
          Cancel
        </Button>

        {/* Show save during submitted to handle scenario when email fails */}
        {!disabled ||
        instance?.status === ReportStatusName.Pending ||
        active?.startsWith(ReportMainMenuOption.Settings) ? (
          <Button onClick={() => submitForm()} disabled={isSubmitting}>
            Save report
            <FaSave />
          </Button>
        ) : (
          <Button
            disabled={isSubmitting}
            onClick={async () => {
              const form = await onGenerate(values, true);
              if (form) updateForm(form);
            }}
          >
            Start next report
            <FaFileCirclePlus />
          </Button>
        )}
      </Show>
      <Show
        visible={
          active?.startsWith(ReportMainMenuOption.Settings) ||
          active?.startsWith(ReportMainMenuOption.Content)
        }
      >
        <Button
          variant="secondary"
          onClick={() => {
            if (active === ReportSettingsMenuOption.Info)
              navigate(`/reports/${values.id}/${ReportSettingsMenuOption.Sections}`);
            else if (active === ReportSettingsMenuOption.Sections)
              navigate(`/reports/${values.id}/${ReportSettingsMenuOption.DataSources}`);
            else if (active === ReportSettingsMenuOption.DataSources)
              navigate(`/reports/${values.id}/${ReportSettingsMenuOption.Preferences}`);
            else if (active === ReportSettingsMenuOption.Preferences)
              navigate(`/reports/${values.id}/${ReportSettingsMenuOption.Send}`);
            else if (active === ReportSettingsMenuOption.Send)
              navigate(`/reports/${values.id}/${ReportMainMenuOption.Content}`);
            else if (active === ReportContentMenuOption.Content)
              navigate(`/reports/${values.id}/${ReportContentMenuOption.Sort}`);
            else if (active === ReportContentMenuOption.Sort)
              navigate(`/reports/${values.id}/${ReportContentMenuOption.Summary}`);
            else if (active === ReportContentMenuOption.Summary)
              navigate(`/reports/${values.id}/${ReportViewMenuOption.View}`);
          }}
        >
          Next Step
          <FaCaretRight className="caret" />
        </Button>
      </Show>
      <Modal
        headerText="Confirm Remove Content"
        body={`Are you sure you wish to remove content from the '${values?.name}' report?`}
        isShowing={isShowingRemove}
        hide={toggleRemove}
        type="delete"
        confirmText="Yes, remove content"
        onConfirm={() => {
          handleRemoveContent();
          toggleRemove();
        }}
      />
      <Modal
        headerText="Send Report to Subscribers"
        body={`Do you want to send an email to the subscribers of this report? ${
          instance?.sentOn ? 'This report has already been sent out by email.' : ''
        }`}
        isShowing={isShowingSend}
        hide={toggleSend}
        type="default"
        confirmText="Yes, send report to subscribers"
        onConfirm={async () => {
          try {
            if (instance) await handlePublish(instance.id);
          } finally {
            toggleSend();
          }
        }}
      />
    </styled.ReportEditActions>
  );
};
