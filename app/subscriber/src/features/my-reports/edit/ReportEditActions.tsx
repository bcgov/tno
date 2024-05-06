import { Action } from 'components/action';
import { Button } from 'components/button';
import { Modal } from 'components/modal';
import React from 'react';
import { FaSave, FaTelegramPlane } from 'react-icons/fa';
import { FaArrowsSpin, FaCaretRight, FaFileCirclePlus, FaTrash } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { useReportInstances } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, ReportStatusName, Show, useModal } from 'tno-core';

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
  /** event to publish the report and send to subscribers. */
  onPublish: () => void;
  /** Event to request starting next report. */
  onGenerate: () => void;
}

/**
 * Provides component that provides action buttons for the report administration (i.e. Cancel, Save, Start next report).
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportEditActions = ({ disabled, onGenerate, onPublish }: IReportEditActionsProps) => {
  const { values, isSubmitting, submitForm, setValues, active } = useReportEditContext();
  const [{ viewReportInstance }] = useReportInstances();
  const [, { storeReportOutput }] = useProfileStore();
  const navigate = useNavigate();
  const { toggle: toggleRemove, isShowing: isShowingRemove } = useModal();

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

  const handleViewReport = React.useCallback(
    async (instanceId: number, regenerate?: boolean | undefined) => {
      try {
        const response = await viewReportInstance(instanceId, regenerate);
        storeReportOutput({ ...response, instanceId });
      } catch {}
    },
    [viewReportInstance, storeReportOutput],
  );

  return (
    <styled.ReportEditActions className="report-edit-actions">
      <Show visible={active?.startsWith(ReportMainMenuOption.Send)}>
        <Col flex="1" alignItems="flex-start">
          <ReportSubscriberExporter />
        </Col>
      </Show>
      <Show visible={active?.startsWith(ReportMainMenuOption.Settings)}>
        <Col flex="1" alignItems="flex-start">
          <ReportExporter />
        </Col>
      </Show>
      <Show
        visible={
          !disabled &&
          !!instance?.content.length &&
          active?.startsWith(ReportMainMenuOption.Content)
        }
      >
        <Col flex="1" alignItems="flex-start">
          <Action icon={<FaTrash />} label="Remove all stories" onClick={() => toggleRemove()} />
        </Col>
      </Show>{' '}
      <Show visible={!instance?.sentOn && active?.startsWith(ReportMainMenuOption.View)}>
        <Col flex="1" alignItems="flex-start">
          <Action
            icon={<FaArrowsSpin className="icon-green" />}
            label="Refresh Preview"
            onClick={() => instance && handleViewReport(instance.id, true)}
          />
        </Col>
      </Show>
      <Button variant="secondary" onClick={() => navigate('/reports')}>
        Cancel
      </Button>
      {/* Show save during submitted to handle scenario when email fails */}
      <Show
        visible={
          !disabled ||
          !instance?.sentOn ||
          active?.startsWith(ReportMainMenuOption.Settings) ||
          active?.startsWith(ReportMainMenuOption.Send)
        }
      >
        <Button
          onClick={() => submitForm()}
          disabled={isSubmitting || instance?.status === ReportStatusName.Submitted}
        >
          Save report
          <FaSave />
        </Button>
      </Show>
      <Show visible={!!instance?.sentOn && !active?.startsWith(ReportMainMenuOption.Settings)}>
        <Button disabled={isSubmitting} onClick={() => onGenerate()} variant="success">
          Start next report
          <FaFileCirclePlus />
        </Button>
      </Show>
      <Show
        visible={
          active?.startsWith(ReportMainMenuOption.Settings) ||
          active?.startsWith(ReportMainMenuOption.Content) ||
          active?.startsWith(ReportMainMenuOption.View)
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
      <Show visible={active?.startsWith(ReportMainMenuOption.Send) && !instance?.sentOn}>
        <Button
          disabled={isSubmitting || !instance || instance?.status === ReportStatusName.Submitted}
          onClick={() => onPublish()}
          variant="success"
        >
          Send to subscribers
          <FaTelegramPlane />
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
    </styled.ReportEditActions>
  );
};
