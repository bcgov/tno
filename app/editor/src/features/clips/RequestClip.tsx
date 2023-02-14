import { FormikForm } from 'components/formik';
import { IIngestModel, IScheduleModel, useApiIngests, useApiIngestSchedules } from 'hooks';
import React from 'react';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  FormikText,
  FormikTimeInput,
  FormPage,
  IOptionItem,
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

import { defaultSchedule } from './constants';
import * as styled from './styled';

export interface IRequestClipProps extends React.HTMLAttributes<HTMLDivElement> {}

/**
 * Provides a form to request generating a clip from a capture source.
 * @returns Component with form to request generating a clip.
 */
export const RequestClip: React.FC<IRequestClipProps> = () => {
  const [{ userInfo }] = useApp();
  const apiIngest = useApiIngests();
  const apiIngestSchedule = useApiIngestSchedules();

  const [sourceId, setSourceId] = React.useState<number>();
  const [ingests, setIngests] = React.useState<IIngestModel[]>([]);
  const [ingest, setIngest] = React.useState<IIngestModel>();

  const sources = ingests.map((i) => new OptionItem(i.source?.code ?? '', i.sourceId));
  const ingestOptions = ingests
    .filter((i) => i.sourceId === sourceId)
    .map((i) => new OptionItem(i.name, i.id));

  React.useEffect(() => {
    apiIngest
      .findIngests({
        isEnabled: true,
        ingestTypeId: [2, 3],
        serviceType: 'clip',
        page: 1,
        quantity: 1000,
      })
      .then((response) => {
        setIngests(response.data.items);
      })
      .catch(); // Errors are handled globally.
  }, [apiIngest]);

  const onSubmit = async (values: IScheduleModel) => {
    if (ingest) {
      // TODO: Test if a duplicate schedule has already been added.
      try {
        const response = await apiIngestSchedule.addSchedule({
          ingestId: ingest.id,
          requestedById: userInfo?.id,
          ...values,
        });
        if (response.status === 200) {
          toast.success(
            `Clip "${values.name}" has successfully been requested.  You will receive a notification when it has been created.`,
          );
        }
      } catch (ex) {
        // Errors are handled globally.
      }
    }
  };

  return (
    <styled.RequestClip>
      <FormPage>
        <FormikForm onSubmit={onSubmit} initialValues={defaultSchedule}>
          {({ values, isSubmitting, resetForm }) => (
            <Col alignContent="center">
              <p className="text">Request a new clip to be created.</p>
              <Show visible={!ingests.length}>
                <p className="error">There are no configured clip ingest services.</p>
              </Show>
              <div className="ingest">
                <Select
                  name="source"
                  label="Source"
                  options={sources}
                  isDisabled={!sources.length}
                  onChange={(newValue) => {
                    const value = (newValue as IOptionItem)?.value;
                    if (value !== undefined) setSourceId(+value);
                    else setSourceId(undefined);
                    setIngest(undefined);
                    resetForm();
                  }}
                />
                <Select
                  name="ingest"
                  label="Ingest"
                  options={ingestOptions}
                  isDisabled={!ingestOptions.length}
                  value={ingestOptions.find((i) => i.value === ingest?.id) ?? ''}
                  onChange={(newValue) => {
                    const ingest = ingests.find((i) => i.id === (newValue as IOptionItem)?.value);
                    setIngest(ingest);
                    resetForm();
                  }}
                />
                <FormikText
                  name="name"
                  label="Name"
                  disabled={!ingest}
                  // value={schedule ? schedule.name : ''}
                  // onChange={(e) => setSchedule({ ...schedule!, name: e.currentTarget.value })}
                />
                <Row alignItems="flex-end">
                  <FormikTimeInput
                    label="Start At"
                    name="startAt"
                    width="7em"
                    placeholder="HH:MM:SS"
                    disabled={!ingest}
                    // value={schedule ? schedule.startAt : ''}
                    // onChange={(e) => {
                    //   const value = e.currentTarget.value;
                    //   // Had to do this because react-text-mask has bugs.
                    //   if (value === '') setSchedule({ ...schedule!, startAt: undefined });
                    //   else if (value !== undefined)
                    //     setSchedule({ ...schedule!, startAt: e.currentTarget.value });
                    // }}
                  />
                  <FormikTimeInput
                    label="Stop At"
                    name="stopAt"
                    width="7em"
                    placeholder="HH:MM:SS"
                    disabled={!ingest}
                    // value={schedule ? schedule.stopAt : ''}
                    // onChange={(e) => {
                    //   const value = e.currentTarget.value;
                    //   // Had to do this because react-text-mask has bugs.
                    //   if (value === '') setSchedule({ ...schedule!, stopAt: undefined });
                    //   else if (value !== undefined)
                    //     setSchedule({ ...schedule!, stopAt: e.currentTarget.value });
                    // }}
                  />
                  <Button
                    type="submit"
                    variant={ButtonVariant.primary}
                    disabled={isSubmitting || !values.name || !values.startAt || !values.stopAt}
                  >
                    Request
                  </Button>
                </Row>
              </div>
            </Col>
          )}
        </FormikForm>
      </FormPage>
    </styled.RequestClip>
  );
};
