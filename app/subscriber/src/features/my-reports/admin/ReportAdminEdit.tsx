import { useFormikContext } from 'formik';
import React from 'react';
import { FaSave, FaTrash } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  hasErrors,
  Modal,
  Row,
  Show,
  Spinner,
  Tab,
  Tabs,
  useModal,
  useTabValidationToasts,
} from 'tno-core';

import { IReportForm } from '../interfaces';
import { ReportAdminSections } from './ReportAdminSections';
import { ReportAdminSettings } from './ReportAdminSettings';

export interface IReportAdminProps {
  path?: string;
}

/**
 * Component provides a way to configure a report.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportAdminEdit: React.FC<IReportAdminProps> = ({
  path: defaultPath = 'settings',
}) => {
  const { values, isSubmitting } = useFormikContext<IReportForm>();
  const navigate = useNavigate();
  const { id, path = defaultPath } = useParams();
  const [{ deleteReport }] = useReports();
  const { toggle, isShowing } = useModal();
  const [savePressed, setSavePressed] = React.useState(false);
  const { setShowValidationToast } = useTabValidationToasts();

  const handleDelete = React.useCallback(
    async (values: IReportForm) => {
      try {
        await deleteReport(values);
        toast.success(`Report '${values.name}' has been deleted.`);
        navigate('/landing/myreports');
      } catch {}
    },
    [deleteReport, navigate],
  );

  return (
    <>
      <Tabs
        tabs={
          <Row flex="1" nowrap>
            <Tab
              showErrorOnSave={{ value: true, savePressed: savePressed }}
              setShowValidationToast={setShowValidationToast}
              hasErrors={hasErrors(props.errors, ['name'])}
              label={
                <Row gap="0.25rem">
                  <Show visible={!!values.id}>
                    <FaTrash
                      className="btn btn-link error"
                      onClick={(e) => {
                        e.stopPropagation();
                        toggle();
                      }}
                    />
                  </Show>
                  Settings
                </Row>
              }
              onClick={() => {
                navigate(`/reports/${id}/settings`);
              }}
              active={path === 'settings'}
            />
            <Tab
              label="Sections"
              onClick={() => {
                navigate(`/reports/${id}/sections`);
              }}
              active={path === 'sections'}
            />
            <Row flex="1" gap="0.5rem">
              <Row flex="1" className="report-name">
                {values.name}
              </Row>
              <Button
                variant={ButtonVariant.success}
                disabled={isSubmitting}
                type="submit"
                title="Save report"
                onClick={() => {
                  setSavePressed(true);
                }}
              >
                <Row gap="0.5rem">
                  <FaSave />
                  {isSubmitting && <Spinner className="white" size="15px" />}
                </Row>
              </Button>
            </Row>
          </Row>
        }
      >
        <Show visible={path === 'settings'}>
          <ReportAdminSettings />
        </Show>
        <Show visible={path === 'sections'}>
          <ReportAdminSections />
          <Row justifyContent="flex-end">
            <Button
              variant={ButtonVariant.success}
              disabled={isSubmitting}
              type="submit"
              title="Save report"
            >
              <Row gap="0.5rem">
                <FaSave />
                {isSubmitting && <Spinner className="white" size="15px" />}
              </Row>
            </Button>
          </Row>
        </Show>
      </Tabs>
      <Modal
        headerText="Confirm Delete"
        body="Are you sure you wish to delete this report?"
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        confirmText="Yes, Remove It"
        onConfirm={async () => {
          await handleDelete(values);
          toggle();
        }}
      />
    </>
  );
};
