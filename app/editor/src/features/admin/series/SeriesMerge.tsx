import { FormikForm } from 'components/formik';
import React from 'react';
import { useLocation, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useSeries } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikSelect,
  FormikText,
  IOptionItem,
  Modal,
  OptionItem,
  Row,
  useModal,
} from 'tno-core';

import { defaultSeries } from './constants';
import { ISeriesForm } from './interfaces';
import * as styled from './styled';
import { toForm } from './utils';

/** The page used to view and edit series the administrative section. */
const SeriesMerge: React.FC = () => {
  const { id } = useParams();
  const [, api] = useSeries();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();
  const [{ series }] = useLookup();

  const [targetSeries, setTargetSeries] = React.useState<ISeriesForm>(
    (state as any)?.series ?? defaultSeries,
  );
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const [mergeSeriesSourceOption, setMergeSeriesSourceOption] = React.useState<IOptionItem>();

  const targetSeriesId = Number(id);

  React.useEffect(() => {
    if (!!targetSeriesId && targetSeries?.id !== targetSeriesId) {
      setTargetSeries({ ...defaultSeries, id: targetSeriesId }); // Do this to stop double fetch.
      api.getSeries(targetSeriesId).then((data) => {
        setTargetSeries(toForm(data));
      });
    }
  }, [api, targetSeries?.id, targetSeriesId]);

  React.useEffect(() => {
    setSeriesOptions(
      series.filter((f) => !f.isOther).map((m: any) => new OptionItem(m.name, m.id, !m.isEnabled)),
    );
  }, [series]);

  return (
    <styled.SeriesMerge>
      <FormikForm
        initialValues={targetSeries}
        onSubmit={(values, { setSubmitting }) => {
          setSubmitting(false);
        }}
      >
        {({ isSubmitting }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <p>You can merge another Show/Program with this one to reduce duplication.</p>
              <p>
                Select the Show/Program from the list below that you want to merge with the current
                Show/Program and then click the [Merge] button.
              </p>
              <p>
                Any content which is associated with the merged Show/Program, will be re-associated
                with the current Show/Program.
              </p>
              <Row>
                <FormikSelect
                  name="seriesId"
                  label="Merge this"
                  isClearable
                  width={FieldSize.Medium}
                  options={seriesOptions.filter((s) => s.value !== targetSeriesId)}
                  onChange={(e: any) => {
                    setMergeSeriesSourceOption(e);
                  }}
                  value={seriesOptions.find((s: any) => s === mergeSeriesSourceOption) ?? ''}
                />
                <FormikText width={FieldSize.Medium} name="name" label="Into this" readOnly />
              </Row>
              <p className="warning">Warning: This action is irreversible.</p>
            </Col>
            <Row justifyContent="center" className="form-inputs">
              <Button
                onClick={toggle}
                variant={ButtonVariant.danger}
                disabled={isSubmitting || mergeSeriesSourceOption === undefined}
              >
                Merge
              </Button>
            </Row>
            <Modal
              headerText="Confirm Merge"
              body={`Are you sure you wish to merge [${mergeSeriesSourceOption?.label}] into [${targetSeries.name}]?`}
              isShowing={isShowing}
              hide={toggle}
              type="default"
              confirmText="Yes, merge them"
              onConfirm={async () => {
                try {
                  await api.mergeSeries(Number(mergeSeriesSourceOption?.value), targetSeriesId);
                  setMergeSeriesSourceOption(undefined);
                  toast.success(`Series/Program merge completed successfully.`);
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.SeriesMerge>
  );
};

export default SeriesMerge;
