import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { useModal, useTooltips } from 'hooks';
import { IWorkOrderModel, WorkOrderStatusName } from 'hooks/api-editor';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useWorkOrders } from 'store/hooks/admin';
import { filterEnabled } from 'store/hooks/lookup/utils';
import {
  Button,
  ButtonVariant,
  Col,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  IconButton,
  OptionItem,
  Show,
} from 'tno-core';
import { Row } from 'tno-core';
import { getEnumStringOptions, getUserOptions } from 'utils';

import { defaultWorkOrder } from './constants';
import * as styled from './styled';

/**
 * Provides a WorkOrder Form to manage, create, update and delete a workOrder.
 * @returns React component containing administrative workOrder form.
 */
export const WorkOrderForm: React.FC = () => {
  const [, api] = useWorkOrders();
  const { id } = useParams();
  const navigate = useNavigate();
  const workOrderId = Number(id);
  const { toggle, isShowing } = useModal();
  const [lookups] = useLookup();
  useTooltips();

  const [workOrder, setWorkOrder] = React.useState<IWorkOrderModel>(defaultWorkOrder);
  const statusOptions = getEnumStringOptions(WorkOrderStatusName);
  const userOptions = getUserOptions(lookups.users, [new OptionItem('None', undefined)]);

  React.useEffect(() => {
    if (!!workOrderId && workOrder?.id !== workOrderId) {
      api.getWorkOrder(workOrderId).then((data) => {
        setWorkOrder(data);
      });
    }
  }, [api, workOrder?.id, workOrderId]);

  const handleSubmit = async (values: IWorkOrderModel) => {
    try {
      const originalId = values.id;
      const result = !workOrder.id
        ? await api.addWorkOrder(values)
        : await api.updateWorkOrder(values);
      setWorkOrder(result);
      toast.success(`Work order has successfully been saved.`);
      if (!originalId) navigate(`/work/orders/${result.id}`);
    } catch {}
  };

  const goToContent = (id: number) => {
    navigate(`/contents/${id}`);
  };

  return (
    <styled.WorkOrderForm>
      <IconButton
        iconType="back"
        label="Back to WorkOrders"
        className="back-button"
        onClick={() => navigate('/work/orders')}
      />
      <FormikForm
        initialValues={workOrder}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ values, isSubmitting }) => (
          <div className="form-container">
            <Row>
              <Col flex="1 1 0">
                <FormikText name="workType" label="Type" disabled />
                <FormikSelect
                  options={filterEnabled(userOptions, values.requestorId)}
                  name="requestorId"
                  label="Requestor"
                  value={userOptions.find((s) => s.value === values.requestorId) || ''}
                />
                <FormikTextArea name="description" label="Description" />
              </Col>
              <Col flex="1 1 0">
                <FormikSelect
                  options={statusOptions}
                  name="status"
                  label="Status"
                  value={statusOptions.find((s) => s.value === values.status) || ''}
                />
                <FormikSelect
                  options={userOptions}
                  name="assignedId"
                  label="Assigned"
                  value={userOptions.find((s) => s.value === values.assignedId) || ''}
                />
                <FormikTextArea name="note" label="Note" />
              </Col>
            </Row>
            <Show visible={!!values.configuration.contentId}>
              <Row>
                <Col flex="1 1 0">
                  <FormikText name="content.headline" label="Content Headline" disabled>
                    <Button
                      variant={ButtonVariant.secondary}
                      onClick={() => goToContent(values.configuration.contentId!)}
                    >
                      Go
                    </Button>
                  </FormikText>
                </Col>
              </Row>
            </Show>
            <Show visible={!!values.id}>
              <Row>
                <Col flex="1 1 0">
                  <FormikText disabled name="updatedBy" label="Updated By" />
                </Col>
                <Col flex="1 1 0">
                  <FormikDatePicker
                    selectedDate={
                      !!values.updatedOn ? moment(values.updatedOn).toString() : undefined
                    }
                    onChange={noop}
                    name="updatedOn"
                    label="Updated On"
                    disabled
                  />
                </Col>
              </Row>
              <Row>
                <Col flex="1 1 0">
                  <FormikText disabled name="createdBy" label="Created By" />
                </Col>
                <Col flex="1 1 0">
                  <FormikDatePicker
                    selectedDate={
                      !!values.createdOn ? moment(values.createdOn).toString() : undefined
                    }
                    onChange={noop}
                    name="createdOn"
                    label="Created On"
                    disabled
                  />
                </Col>
              </Row>
            </Show>
            <Row justifyContent="center" className="form-inputs">
              <Button type="submit" disabled={isSubmitting}>
                Save
              </Button>
              <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                Delete
              </Button>
            </Row>
            <Modal
              headerText="Confirm Removal"
              body="Are you sure you wish to remove this workOrder?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteWorkOrder(workOrder);
                  toast.success(`Work order has successfully been deleted.`);
                  navigate('/work/orders');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.WorkOrderForm>
  );
};
