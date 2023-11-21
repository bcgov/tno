import { useFormikContext } from 'formik';
import React from 'react';
import { DragDropContext, DropResult, ResponderProvided } from 'react-beautiful-dnd';
import { FaRegClock, FaSave } from 'react-icons/fa';
import { FaArrowsSpin, FaFileExcel, FaGear } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useReportInstances, useReports } from 'store/hooks';
import { Button, ButtonVariant, Col, Modal, Row, Show, useModal } from 'tno-core';

import { IReportForm } from '../interfaces';
import { getLastPublishedOn, getLastSent, moveContent, toForm } from '../utils';
import { calcNextSend } from './../utils/calcNextSend';
import { ReportSection } from './components';

export const ReportSnapshotEdit: React.FC = () => {
  const navigate = useNavigate();
  const { values, isSubmitting, setValues, setFieldValue, submitForm } =
    useFormikContext<IReportForm>();
  const [{ generateReport }] = useReports();
  const [{ exportReport }] = useReportInstances();

  const { isShowing, toggle } = useModal();

  const instance = values.instances.length ? values.instances[0] : null;

  const handleRegenerate = React.useCallback(
    async (values: IReportForm) => {
      try {
        const result = await generateReport(values.id, true);
        setValues(toForm(result, values));
        toast.success('Report has been regenerated');
      } catch {}
    },
    [generateReport, setValues],
  );

  const handleExport = React.useCallback(async () => {
    try {
      if (instance?.id) {
        const filename = values.name.replace(/[^a-zA-Z0-9 ]/g, '');
        await toast.promise(exportReport(instance?.id, filename), {
          pending: 'Downloading file',
          success: 'Download complete',
          error: 'Download failed',
        });
      }
    } catch {}
  }, [exportReport, instance?.id, values.name]);

  /** function that runs after a user drops an item in the list */
  const handleDrop = React.useCallback(
    (result: DropResult, provided: ResponderProvided) => {
      if (instance) {
        const newItems = moveContent(result, instance.content);
        setFieldValue(`instances.0.content`, newItems);
      }
    },
    [instance, setFieldValue],
  );

  return (
    <Col>
      <Col className="header">
        <Row gap="0.25rem">
          <Col flex="1">
            <h2>Edit Report</h2>
          </Col>
          <Button
            variant={ButtonVariant.secondary}
            disabled={isSubmitting}
            title="Export to Excel"
            onClick={() => handleExport()}
          >
            <FaFileExcel />
          </Button>
          <Button
            variant={ButtonVariant.secondary}
            disabled={isSubmitting}
            title="Configure"
            onClick={() => navigate(`/reports/${values.id}/sections`)}
          >
            <FaGear />
          </Button>
          <Button
            variant={ButtonVariant.secondary}
            disabled={isSubmitting}
            title="Regenerate"
            onClick={() => toggle()}
          >
            <FaArrowsSpin />
          </Button>
          <Button
            variant={ButtonVariant.success}
            disabled={isSubmitting}
            title="Save changes"
            onClick={() => submitForm()}
          >
            <FaSave />
          </Button>
        </Row>
        <Row gap="0.5rem" className="frm-in" nowrap alignItems="center">
          <Col flex="1">
            <Row>
              <Col flex="1" justifyItems="center">
                <label>Last sent:</label>
              </Col>
              <Col flex="1" justifyItems="center">
                {getLastSent(values)}
              </Col>
            </Row>
            <Row>
              <Col flex="1" justifyItems="center">
                <label>Published:</label>
              </Col>
              <Col flex="1" justifyItems="center">
                {getLastPublishedOn(values)}
              </Col>
            </Row>
          </Col>
          <Row flex="1" alignItems="center">
            <Row flex="2" gap="0.5rem" alignItems="center">
              <FaRegClock />
              <label>Next scheduled send:</label>
            </Row>
            <Col flex="1" justifyItems="center">
              {calcNextSend(values)}
            </Col>
          </Row>
        </Row>
      </Col>
      <Row>
        <Col flex="1">
          <h2>{values.name}</h2>
        </Col>
      </Row>
      <Col className="sections" gap="0.5rem">
        <DragDropContext onDragEnd={handleDrop}>
          {values.sections.map((section, index) => (
            <ReportSection key={section.id} index={index} />
          ))}
        </DragDropContext>
      </Col>
      <Show visible={!!values.sections.length}>
        <Row justifyContent="flex-end">
          <Button
            variant={ButtonVariant.success}
            disabled={isSubmitting}
            title="Save changes"
            onClick={() => submitForm()}
          >
            <FaSave />
          </Button>
        </Row>
      </Show>
      <Modal
        headerText="Regenerate Snapshot"
        body="Regenerating a snapshot will rerun all filters and update content in the report.  Do you want to proceed?"
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, Regenerate It"
        onConfirm={async () => {
          try {
            await handleRegenerate(values);
          } finally {
            toggle();
          }
        }}
      />
    </Col>
  );
};
