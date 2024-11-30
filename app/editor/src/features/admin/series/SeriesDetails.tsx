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
  getSortableOptions,
  getSourceOptions,
  IOptionItem,
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
const SeriesDetails: React.FC = () => {
  const { id } = useParams();
  const [, api] = useSeries();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();
  const [{ sources }] = useLookup();
  const [lookups] = useLookup();
  const navigate = useNavigate();

  const [sourceOptions, setSourceOptions] = React.useState<IOptionItem[]>([]);
  const [targetSeries, setTargetSeries] = React.useState<ISeriesForm>(
    (state as any)?.series ?? defaultSeries,
  );

  const targetSeriesId = Number(id);
  const mediaTypesForSearchMapping = getSortableOptions(lookups.mediaTypes);

  React.useEffect(() => {
    if (!!targetSeriesId && targetSeries?.id !== targetSeriesId) {
      setTargetSeries({ ...defaultSeries, id: targetSeriesId }); // Do this to stop double fetch.
      api.getSeries(targetSeriesId).then((data) => {
        setTargetSeries(toForm(data));
      });
    }
  }, [api, targetSeries?.id, targetSeriesId]);

  React.useEffect(() => {
    setSourceOptions(getSourceOptions(sources, [new OptionItem('Any Source', '')]));
  }, [sources]);

  const handleSubmit = async (values: ISeriesForm) => {
    try {
      const originalId = values.id;
      const model = toModel(values);
      const result = !targetSeries.id ? await api.addSeries(model) : await api.updateSeries(model);
      setTargetSeries(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/programs/${result.id}`);
    } catch {}
  };

  return (
    <styled.SeriesDetails>
      <FormikForm
        initialValues={targetSeries}
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
              <FormikSelect
                label="Media Type Search Group"
                isMulti
                name="mediaTypeSearchMappings"
                tooltip="Select the Media Types that this Show/Program will show up under the Advanced search UI."
                options={mediaTypesForSearchMapping}
                value={
                  values.mediaTypeSearchMappings?.map((mt) =>
                    mediaTypesForSearchMapping.find((o) => o.value === mt.id),
                  ) ?? []
                }
                onChange={(newValue) => {
                  const options = newValue as IOptionItem[];
                  setFieldValue(
                    'mediaTypeSearchMappings',
                    lookups.mediaTypes.filter((mt) => options.some((o) => o.value === mt.id)),
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
              <Col gap="0.5rem">
                <Row>
                  <FormikCheckbox
                    label="Is Other"
                    name="isOther"
                    tooltip="Shows/Programs marked as 'Other' will not display in the Advanced Search filter
                  *OR* the main Show/Programs drop down list on the main content page."
                  />
                </Row>
                <FormikCheckbox label="Is Enabled" name="isEnabled" />
                <FormikCheckbox label="Use in Topics" name="useInTopics" />
                <FormikCheckbox label="Automatically transcribe when saved" name="autoTranscribe" />
                <FormikCheckbox label="Is CBRA Source" name="isCBRASource" />
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
              body="Are you sure you wish to remove this show/program?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteSeries(toModel(targetSeries));
                  toast.success(`${targetSeries.name} has successfully been deleted.`);
                  navigate('/admin/programs');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.SeriesDetails>
  );
};

export default SeriesDetails;
