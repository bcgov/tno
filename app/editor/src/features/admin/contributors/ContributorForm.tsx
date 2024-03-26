import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useContributors } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getSourceOptions,
  IconButton,
  IOptionItem,
  Modal,
  OptionItem,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { defaultContributor } from './constants';
import { IContributorForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';

/** The page used to view and edit contributors the administrative section. */
const ContributorForm: React.FC = () => {
  const { id } = useParams();
  const [, api] = useContributors();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();
  const [{ sources }] = useLookup();
  const navigate = useNavigate();

  const [sourceOptions, setSourceOptions] = React.useState<IOptionItem[]>([]);
  const [contributors, setContributor] = React.useState<IContributorForm>(
    (state as any)?.contributors ?? defaultContributor,
  );

  const contributorsId = Number(id);

  React.useEffect(() => {
    if (!!contributorsId && contributors?.id !== contributorsId) {
      setContributor({ ...defaultContributor, id: contributorsId }); // Do this to stop double fetch.
      api.getContributor(contributorsId).then((data) => {
        setContributor(toForm(data));
      });
    }
  }, [api, contributors?.id, contributorsId]);

  React.useEffect(() => {
    setSourceOptions(getSourceOptions(sources, [new OptionItem('Any Source', '')]));
  }, [sources]);

  const handleSubmit = async (values: IContributorForm) => {
    try {
      const originalId = values.id;
      const result = !contributors.id
        ? await api.addContributor(toModel(values))
        : await api.updateContributor(toModel(values));
      setContributor(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/contributors/${result.id}`);
    } catch {}
  };

  return (
    <styled.ContributorForm>
      <IconButton
        iconType="back"
        label="Back to Columns/Pundits"
        className="back-button"
        onClick={() => navigate('/admin/contributors')}
      />
      <FormikForm
        initialValues={contributors}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText width={FieldSize.Large} name="name" label="Name" />
              <FormikText
                width={FieldSize.Large}
                name="aliases"
                label="Aliases"
                tooltip={
                  'Create a comma separated list of values to search by. ex.) "John Doe", "J. Doe", "Sir Doe"'
                }
              />

              <FormikSelect
                label="Source"
                name="sourceId"
                options={sourceOptions}
                onChange={(newValue: any) => {
                  // Use the source.code to set the Kafka topic.
                  setFieldValue('sourceId', newValue.value ?? '');
                }}
                clearValue=""
              />
              <FormikTextArea name="description" label="Description" width={FieldSize.Large} />
              <FormikText
                width={FieldSize.Tiny}
                name="sortOrder"
                label="Sort Order"
                type="number"
                className="sort-order"
              />
              <Col gap="0.5rem">
                <FormikCheckbox label="Is Enabled" name="isEnabled" />
                <FormikCheckbox label="Is Press" name="isPress" />
                <FormikCheckbox label="Automatically transcribe when saved" name="autoTranscribe" />
              </Col>
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
              body="Are you sure you wish to remove this columnist/pundit?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteContributor(toModel(contributors));
                  toast.success(`${contributors.name} has successfully been deleted.`);
                  navigate('/admin/contributors');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.ContributorForm>
  );
};

export default ContributorForm;
