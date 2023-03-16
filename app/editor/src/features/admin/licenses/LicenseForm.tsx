import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { ILicenseModel, useModal } from 'hooks';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLicenses } from 'store/hooks/admin';
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

import { defaultLicense } from './constants';
import * as styled from './styled';

/** The page used to view and edit tags in the administrative section. */
export const LicenseForm: React.FC = () => {
  const [, api] = useLicenses();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();

  const [license, setLicense] = React.useState<ILicenseModel>(
    (state as any)?.license ?? defaultLicense,
  );

  const licenseId = Number(id);
  const { toggle, isShowing } = useModal();

  React.useEffect(() => {
    if (!!licenseId && license?.id !== licenseId) {
      setLicense({ ...defaultLicense, id: licenseId }); // Do this to stop double fetch.
      api.getLicense(licenseId).then((data) => {
        setLicense(data);
      });
    }
  }, [api, license?.id, licenseId]);

  const handleSubmit = async (values: ILicenseModel) => {
    try {
      const originalId = values.id;
      const result = !license.id ? await api.addLicense(values) : await api.updateLicense(values);
      setLicense(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/licences/${result.id}`);
    } catch {}
  };

  return (
    <styled.LicenseForm>
      <IconButton
        iconType="back"
        label="Back to Licences"
        className="back-button"
        onClick={() => navigate('/admin/licences')}
      />
      <FormikForm
        initialValues={license}
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
                name="ttl"
                label="Time to Live (days)"
                tooltip="How long content is stored before it is archived or deleted"
                type="number"
                className="sort-order"
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
              body="Are you sure you wish to remove this Licence?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteLicense(license);
                  toast.success(`${license.name} has successfully been deleted.`);
                  navigate('/admin/licences');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.LicenseForm>
  );
};
