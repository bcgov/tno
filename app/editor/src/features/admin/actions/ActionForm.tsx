import { FormikForm } from 'components/formik';
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
  ContentTypeName,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IActionModel,
  IconButton,
  IOptionItem,
  LabelPosition,
  Modal,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { defaultAction } from './constants';
import * as styled from './styled';

/**
 * The page used to view and edit actions.
 * @returns Component.
 */
export const ActionForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [, api] = useActions();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();

  const [action, setAction] = React.useState<IActionModel>((state as any)?.action ?? defaultAction);

  const actionId = Number(id);
  const contentTypeOptions = getEnumStringOptions(ContentTypeName);

  React.useEffect(() => {
    if (!!actionId && action?.id !== actionId) {
      setAction({ ...defaultAction, id: actionId }); // Do this to stop double fetch.
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
        {({ isSubmitting, values, setFieldValue }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText width={FieldSize.Large} name="name" label="Name" />
              <FormikTextArea name="description" label="Description" width={FieldSize.Large} />
              <FormikSelect
                name="contentTypes"
                label="Content Types"
                isMulti
                options={contentTypeOptions}
                value={contentTypeOptions.filter((ct) =>
                  values.contentTypes.includes(ct.value as ContentTypeName),
                )}
                onChange={(newValue) => {
                  const options = newValue as IOptionItem[];
                  setFieldValue(
                    'contentTypes',
                    options.map((o) => o.value),
                  );
                }}
              />
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
              <Show visible={!!values.id}>
                <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                  Delete
                </Button>
              </Show>
            </Row>
            <Modal
              headerText="Confirm Removal"
              body="Are you sure you wish to remove this action?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteAction(action);
                  toast.success(`${action.name} has successfully been deleted.`);
                  navigate('/admin/actions');
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
