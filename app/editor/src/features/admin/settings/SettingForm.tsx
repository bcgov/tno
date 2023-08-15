import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useSettings } from 'store/hooks/admin';
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
  ISettingModel,
  LabelPosition,
  Modal,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { defaultSetting } from './constants';
import * as styled from './styled';

/**
 * The page used to view and edit settings.
 * @returns Component.
 */
export const SettingForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [, api] = useSettings();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();

  const [setting, setSetting] = React.useState<ISettingModel>(
    (state as any)?.setting ?? defaultSetting,
  );

  const settingId = Number(id);

  React.useEffect(() => {
    if (!!settingId && setting?.id !== settingId) {
      setSetting({ ...defaultSetting, id: settingId }); // Do this to stop double fetch.
      api.getSetting(settingId).then((data) => {
        setSetting(data);
      });
    }
  }, [api, setting?.id, settingId]);

  const handleSubmit = async (values: ISettingModel) => {
    try {
      const originalId = values.id;
      const result = !setting.id ? await api.addSetting(values) : await api.updateSetting(values);
      setSetting(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/settings/${result.id}`);
    } catch {}
  };

  return (
    <styled.SettingForm>
      <IconButton
        iconType="back"
        label="Back to settings"
        className="back-button"
        onClick={() => navigate('/admin/settings')}
      />
      <FormikForm
        initialValues={setting}
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
              <FormikText name="value" label="Value" />
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
              body="Are you sure you wish to remove this setting?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteSetting(setting);
                  toast.success(`${setting.name} has successfully been deleted.`);
                  navigate('/admin/settings');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.SettingForm>
  );
};
