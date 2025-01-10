import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { FaArrowAltCircleRight, FaRegListAlt } from 'react-icons/fa';
import { useApp } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  Error,
  FieldSize,
  ITimeTrackingModel,
  Modal,
  Row,
  Text,
  useModal,
} from 'tno-core';

import { TimeLogTable } from '../../TimeLogTable';
import * as styled from './styled';

export interface ITimeLogSectionProps {
  prepTimeRequired?: boolean;
}

/**
 * TimeLogSection contains the input for time tracking gor a content item.
 */
export const TimeLogSection: React.FC<ITimeLogSectionProps> = ({ prepTimeRequired = false }) => {
  const { values, setFieldValue, errors } = useFormikContext<IContentForm>();
  const { isShowing, toggle } = useModal();
  const [{ userInfo }] = useApp();

  const [prep, setPrep] = React.useState<number | ''>('');

  const userId = userInfo?.id ?? 0;
  const effort = values.timeTrackings.reduce((result, entry) => result + entry.effort, 0);

  const addTime = React.useCallback(
    (value: number | string) => {
      if (!!values.timeTrackings && typeof value === 'number' && value > 0) {
        const entry: ITimeTrackingModel = {
          id: 0,
          contentId: values.id,
          userId: userId,
          activity: !!values.id ? 'Updated' : 'Created',
          effort: +value,
          createdOn: moment().toLocaleString(),
        };
        setFieldValue('timeTrackings', [...values.timeTrackings, entry]);
        setPrep('');
      }
    },
    [setFieldValue, userId, values.id, values.timeTrackings],
  );

  return (
    <styled.TimeLogSection className="multi-group">
      <Col>
        <Row alignItems="center">
          <Text
            width={FieldSize.Small}
            name="prep"
            label="Prep time (minutes)"
            value={prep}
            type="number"
            disabled={!userId}
            required={prepTimeRequired && !values.timeTrackings.some((entry) => entry.id === 0)}
            onChange={(e) => {
              const value = parseFloat(e.target.value);
              if (value > 0) setPrep(value);
            }}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                addTime(prep);
                e.preventDefault();
                return false;
              }
            }}
          />
          <FaArrowAltCircleRight
            className="action-button"
            onClick={() => {
              addTime(prep);
            }}
          />
          <Row className="disabled-section">
            <Text
              name="efforts"
              label="Total minutes"
              disabled
              className="total-mins"
              width={FieldSize.Small}
              value={effort?.toString()}
            />
            <FaRegListAlt
              className="action-button"
              onClick={() => {
                if (!!values.timeTrackings) setFieldValue('timeTrackings', values.timeTrackings);
                toggle();
              }}
            />
          </Row>
        </Row>
        <Error error={errors.prep} />
      </Col>
      <Modal
        hide={toggle}
        isShowing={isShowing}
        headerText="Prep Time Log"
        component={<TimeLogTable />}
        // body=""
        type="custom"
        customButtons={
          <Button variant={ButtonVariant.secondary} onClick={toggle}>
            Close
          </Button>
        }
      />
    </styled.TimeLogSection>
  );
};
