import { useFormikContext } from 'formik';
import React from 'react';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks/admin';
import {
  Checkbox,
  FlexboxTable,
  IReportInstanceModel,
  IReportModel,
  Modal,
  ReportStatusName,
  useModal,
} from 'tno-core';

import { instanceColumns } from './constants';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const ReportFormInstance: React.FC = () => {
  const { values } = useFormikContext<IReportModel>();
  const [, { findInstancesForReportId, deleteReportInstance, publishReportInstance }] =
    useReports();
  const { toggle: toggleDelete, isShowing: showDelete } = useModal();
  const { toggle: toggleResend, isShowing: showResend } = useModal();

  const [instances, setInstances] = React.useState<IReportInstanceModel[]>([]);
  const [instance, setInstance] = React.useState<IReportInstanceModel>();
  const [resend, setResend] = React.useState(false);

  React.useEffect(() => {
    findInstancesForReportId(values.id).then((instances) => {
      setInstances(instances ?? []);
    });
    // Only load the report instances when clicking on the tab.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleDelete = React.useCallback(
    async (model: IReportInstanceModel) => {
      try {
        await deleteReportInstance(model);
        setInstances((instances) => instances.filter((i) => i.id !== model.id));
      } catch {
        // Handled globally.
      }
    },
    [deleteReportInstance],
  );

  const handleResend = React.useCallback(
    async (model: IReportInstanceModel, resend: boolean) => {
      try {
        const instance = await publishReportInstance(model, resend);
        setInstances((instances) =>
          instances.map((i) => {
            if (i.id === instance.id) return instance;
            return i;
          }),
        );
      } catch {
        // Handled globally.
      }
    },
    [publishReportInstance],
  );

  const handlePreview = React.useCallback((model: IReportInstanceModel) => {
    window.open(`/report/instances/${model.id}/view`, '_blank');
  }, []);

  return (
    <>
      <h2>{values.name}</h2>
      <p>
        A report instance is a when a user has published a report, or is actively adding content to
        a report. Each instance provides an historical archive of what content was included in the
        report, and information related to the success or failure of the sent emails.
      </p>
      <FlexboxTable
        rowId="id"
        data={instances}
        columns={instanceColumns({
          onDelete: (instance) => {
            setInstance(instance);
            toggleDelete();
          },
          onResend: (instance) => {
            setResend(
              ![ReportStatusName.Cancelled, ReportStatusName.Failed].includes(instance.status),
            );
            setInstance(instance);
            toggleResend();
          },
          onPreview: handlePreview,
        })}
        showActive={false}
        showPaging={false}
      />
      <Modal
        headerText="Confirm Removal"
        body="Are you sure you wish to remove this report instance?"
        isShowing={showDelete}
        hide={toggleDelete}
        type="delete"
        confirmText="Yes, Remove It"
        onConfirm={async () => {
          try {
            if (instance) {
              await handleDelete(instance);
              toast.success(`Report instance has successfully been deleted.`);
            }
          } catch {
            // Globally handled
          } finally {
            toggleDelete();
          }
        }}
      />
      <Modal
        headerText="Confirm Resend"
        component={
          <div>
            <p>Are you sure you wish to resend this report to subscribers?</p>
            <p>
              Leave unchecked if you only want to send to subscribers who have not received it yet.
            </p>
            <Checkbox
              name="resend"
              label="Resend to all subscribers"
              checked={resend}
              onChange={(e) => setResend(e.target.checked)}
            />
          </div>
        }
        isShowing={showResend}
        hide={toggleResend}
        type="default"
        confirmText="Yes, Resend It"
        onConfirm={async () => {
          try {
            if (instance) {
              await handleResend(instance, resend);
              toast.success(`Request to resend Report instance has successfully been sent.`);
            }
          } catch {
            // Globally handled
          } finally {
            toggleResend();
          }
        }}
      />
    </>
  );
};
