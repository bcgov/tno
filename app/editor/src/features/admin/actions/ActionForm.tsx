import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { IActionModel } from 'hooks/api-editor';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useActions } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  IconButton,
  LabelPosition,
  Row,
  Show,
} from 'tno-core';

import { defaultAction } from './constants';
import * as styled from './styled';

/** The page used to view and edit series the administrative section. */
export const ActionForm: React.FC = () => {
  const [, api] = useActions();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const actionId = Number(id);
  const [action, setAction] = React.useState<IActionModel>((state as any)?.action ?? defaultAction);

  const { toggle, isShowing } = useModal();

  React.useEffect(() => {
    if (!!actionId && action?.id !== actionId) {
      api.getAction(actionId).then((data) => {
        setAction(data);
      });
    }
  }, [api, action?.id, actionId]);

  const handleSubmit = async (values: IActionModel) => {
    try {
      const originalId = values.id;
      const result = !action.id ? await api.addAction(values) : await api.updateAction(values);
      setAction(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/actions/${result.id}`);
    } catch {}
  };

  return (
    <styled.ActionForm>
      <IconButton
        iconType="back"
        label="Back to actions"
        className="back-button"
        onClick={() => navigate('/admin/actions')}
      />
      <FormikForm
        initialValues={action}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText width={FieldSize.Large} name="name" label="Name" />
              <FormikTextArea name="description" label="Description" width={FieldSize.Large} />
              <FormikText
                width={FieldSize.Tiny}
                name="sortOrder"
                label="Sort Order"
                type="number"
                className="sort-order"
              />
              <FormikCheckbox
                labelPosition={LabelPosition.Top}
                label="Is Enabled"
                name="isEnabled"
              />
              <Show visible={!!values.id}>
                <Row>
                  <FormikText
                    width={FieldSize.Small}
                    disabled
                    name="updatedBy"
                    label="Updated By"
                  />
                  <FormikDatePicker
                    selectedDate={
                      !!values.updatedOn ? moment(values.updatedOn).toString() : undefined
                    }
                    onChange={noop}
                    name="updatedOn"
                    label="Updated On"
                    disabled
                    width={FieldSize.Small}
                  />
                </Row>
                <Row>
                  <FormikText
                    width={FieldSize.Small}
                    disabled
                    name="createdBy"
                    label="Created By"
                  />
                  <FormikDatePicker
                    selectedDate={
                      !!values.createdOn ? moment(values.createdOn).toString() : undefined
                    }
                    onChange={noop}
                    name="createdOn"
                    label="Created On"
                    disabled
                    width={FieldSize.Small}
                  />
                </Row>
              </Show>
            </Col>
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
              body="Are you sure you wish to remove this series?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteAction(action);
                  toast.success(`${action.name} has successfully been deleted.`);
                  navigate('/admin/series');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.ActionForm>
  );
};
