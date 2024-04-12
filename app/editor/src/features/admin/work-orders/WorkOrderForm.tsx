import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useUsers, useWorkOrders } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  filterEnabledOptions,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  getUserOptions,
  IconButton,
  Modal,
  OptionItem,
  Row,
  Show,
  useModal,
  WorkOrderStatusName,
} from 'tno-core';

import { defaultWorkOrder } from './constants';
import { IWorkOrderForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';

/**
 * Provides a WorkOrder Form to manage, create, update and delete a workOrder.
 * @returns React component containing administrative workOrder form.
 */
const WorkOrderForm: React.FC = () => {
  const [, api] = useWorkOrders();
  const { id } = useParams();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();
  const [{ users }, { findUsers }] = useUsers();

  const [workOrder, setWorkOrder] = React.useState<IWorkOrderForm>(defaultWorkOrder);
  const [userOptions, setUserOptions] = React.useState(
    getUserOptions(users.items, [new OptionItem('None', undefined)]),
  );

  const workOrderId = Number(id);
  const statusOptions = getEnumStringOptions(WorkOrderStatusName);

  React.useEffect(() => {
    if (!!workOrderId && workOrder?.id !== workOrderId) {
      setWorkOrder({ ...defaultWorkOrder, id: workOrderId }); // Do this to stop double fetch.
      api.getWorkOrder(workOrderId).then((data) => {
        setWorkOrder(toForm(data));
        return findUsers({ includeUserId: data.requestorId, quantity: 50 });
      });
    }
  }, [api, findUsers, workOrder?.id, workOrderId]);

  React.useEffect(() => {
    setUserOptions(getUserOptions(users.items, [new OptionItem('None', undefined)]));
  }, [users]);

  const handleSubmit = async (values: IWorkOrderForm) => {
    try {
      const originalId = values.id;
      const result = !workOrder.id
        ? await api.addWorkOrder(toModel(values))
        : await api.updateWorkOrder(toModel(values));
      setWorkOrder(toForm(result));
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
        onClick={() => navigate('/admin/work/orders')}
      />
      <FormikForm
        initialValues={workOrder}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ values, isSubmitting, setFieldValue }) => (
          <div className="form-container">
            <Row>
              <Col flex="1 1 0">
                <FormikText name="workType" label="Type" disabled />
                <FormikSelect
                  options={filterEnabledOptions(userOptions, values.requestorId)}
                  name="requestorId"
                  label="Requestor"
                  isClearable={false}
                  value={userOptions.find((s) => s.value === values.requestorId) || ''}
                  onChange={(newValue) => {
                    const option = newValue as OptionItem;
                    setFieldValue('requestorId', option.value);
                  }}
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
                  onChange={(newValue) => {
                    const option = newValue as OptionItem;
                    setFieldValue('assignedId', option.value);
                  }}
                />
                <FormikTextArea name="note" label="Note" />
              </Col>
            </Row>
            <Show visible={!!values.configuration.contentId}>
              <Row>
                <Col flex="1 1 0">
                  <FormikText name="configuration.headline" label="Content Headline" disabled>
                    <Button
                      variant={ButtonVariant.secondary}
                      onClick={() =>
                        goToContent(values.configuration.contentId ?? values.contentId ?? 0)
                      }
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
              <Show visible={!!values.id}>
                <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                  Delete
                </Button>
              </Show>
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
                  await api.deleteWorkOrder(toModel(values));
                  toast.success(`Work order has successfully been deleted.`);
                  navigate('/admin/work/orders');
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

export default WorkOrderForm;
