import { IconButton, LabelPosition } from 'components/form';
import { FormikCheckbox, FormikForm, FormikText, FormikTextArea } from 'components/formik';
import { FormikDatePicker } from 'components/formik/datepicker';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { ISeriesModel } from 'hooks/api-editor';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useSeries } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, FieldSize, Row, Show } from 'tno-core';

import { defaultSeries } from './constants';
import * as styled from './styled';

/** The page used to view and edit series the administrative section. */
export const SeriesForm: React.FC = () => {
  const [, api] = useSeries();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();
  const seriesId = Number(id);
  const [series, setSeries] = React.useState<ISeriesModel>((state as any)?.series ?? defaultSeries);

  const { toggle, isShowing } = useModal();

  React.useEffect(() => {
    if (!!seriesId && series?.id !== seriesId) {
      api.getSeries(seriesId).then((data) => {
        setSeries(data);
      });
    }
  }, [api, series?.id, seriesId]);

  const handleSubmit = async (values: ISeriesModel) => {
    try {
      const originalId = values.id;
      const result = !series.id ? await api.addSeries(values) : await api.updateSeries(values);
      setSeries(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/series/${result.id}`);
    } catch {}
  };

  return (
    <styled.SeriesForm>
      <IconButton
        iconType="back"
        label="Back to Series"
        className="back-button"
        onClick={() => navigate('/admin/series')}
      />
      <FormikForm
        initialValues={series}
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
                  await api.deleteSeries(series);
                  toast.success(`${series.name} has successfully been deleted.`);
                  navigate('/admin/series');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.SeriesForm>
  );
};
