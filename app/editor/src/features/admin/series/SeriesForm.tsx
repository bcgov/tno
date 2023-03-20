import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useSeries } from 'store/hooks/admin';
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
  LabelPosition,
  Modal,
  OptionItem,
  Row,
  Show,
  useModal,
} from 'tno-core';

import { defaultSeries } from './constants';
import { ISeriesForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';

/** The page used to view and edit series the administrative section. */
export const SeriesForm: React.FC = () => {
  const { id } = useParams();
  const [, api] = useSeries();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();
  const [{ sources }] = useLookup();
  const navigate = useNavigate();

  const [sourceOptions, setSourceOptions] = React.useState<IOptionItem[]>([]);
  const [series, setSeries] = React.useState<ISeriesForm>((state as any)?.series ?? defaultSeries);

  const seriesId = Number(id);

  React.useEffect(() => {
    if (!!seriesId && series?.id !== seriesId) {
      setSeries({ ...defaultSeries, id: seriesId }); // Do this to stop double fetch.
      api.getSeries(seriesId).then((data) => {
        setSeries(toForm(data));
      });
    }
  }, [api, series?.id, seriesId]);

  React.useEffect(() => {
    setSourceOptions(getSourceOptions(sources, [new OptionItem('Any Source', '')]));
  }, [sources]);

  const handleSubmit = async (values: ISeriesForm) => {
    try {
      const originalId = values.id;
      const result = !series.id
        ? await api.addSeries(toModel(values))
        : await api.updateSeries(toModel(values));
      setSeries(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/programs/${result.id}`);
    } catch {}
  };

  return (
    <styled.SeriesForm>
      <IconButton
        iconType="back"
        label="Back to Show/Programs"
        className="back-button"
        onClick={() => navigate('/admin/programs')}
      />
      <FormikForm
        initialValues={series}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <FormikText width={FieldSize.Large} name="name" label="Name" />

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
              <FormikCheckbox
                labelPosition={LabelPosition.Top}
                label="Is Enabled"
                name="isEnabled"
              />
              <FormikCheckbox
                labelPosition={LabelPosition.Top}
                label="Use in Topics"
                name="useInTopics"
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
              body="Are you sure you wish to remove this series?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteSeries(toModel(series));
                  toast.success(`${series.name} has successfully been deleted.`);
                  navigate('/admin/programs');
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
