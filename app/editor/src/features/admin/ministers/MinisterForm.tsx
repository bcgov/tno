import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useMinisters } from 'store/hooks/admin';
import { IMinisterModel, Modal, useModal } from 'tno-core';
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

import { defaultMinister } from './constants';
import * as styled from './styled';

/**
 * Admin form for minister configuration.
 * @returns Component.
 */
const MinisterForm: React.FC = () => {
  const [, api] = useMinisters();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const { toggle, isShowing } = useModal();

  const ministerId = Number(id);
  const [minister, setMinister] = React.useState<IMinisterModel>(
    (state as any)?.minister ?? defaultMinister,
  );

  React.useEffect(() => {
    if (!!ministerId && minister?.id !== ministerId) {
      setMinister({ ...defaultMinister, id: ministerId }); // Do this to stop double fetch.
      api.getMinister(ministerId).then((data) => {
        setMinister(data);
      });
    }
  }, [api, minister?.id, ministerId]);

  const handleSubmit = async (values: IMinisterModel) => {
    try {
      const originalId = values.id;
      const result = !minister.id
        ? await api.addMinister(values)
        : await api.updateMinister(values);
      setMinister(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/ministers/${result.id}`);
    } catch {}
  };

  return (
    <styled.MinisterForm>
      <IconButton
        iconType="back"
        label="Back to Ministers"
        className="back-button"
        onClick={() => navigate('/admin/ministers')}
      />
      <FormikForm
        initialValues={minister}
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
              <FormikTextArea name="position" label="Position" width={FieldSize.Large} />
              {/* <FormikText name="aliases" label="Aliases" width={FieldSize.Large} /> */}
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
              body="Are you sure you wish to remove this Minister?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteMinister(minister);
                  toast.success(`${minister.name} has successfully been deleted.`);
                  navigate('/admin/ministers');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.MinisterForm>
  );
};

export default MinisterForm;
