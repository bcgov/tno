import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaRegListAlt } from 'react-icons/fa';
import { useApp } from 'store/hooks';
import { Button, ButtonVariant, Col, Error, FieldSize, Modal, Row, Text, useModal } from 'tno-core';

import { TimeLogTable } from '../../TimeLogTable';
import * as styled from './styled';

export interface ITimeLogSectionProps {
  prepTimeRequired?: boolean;
  prepTime?: string;
  onPrepTimeChanged: (value: string) => void;
}

/**
 * TimeLogSection contains the input for time tracking gor a content item.
 */
export const TimeLogSection: React.FC<ITimeLogSectionProps> = ({
  prepTimeRequired = false,
  prepTime = '',
  onPrepTimeChanged,
}) => {
  const { values, setFieldValue, errors } = useFormikContext<IContentForm>();
  const { isShowing, toggle } = useModal();
  const [{ userInfo }] = useApp();

  const userId = userInfo?.id ?? 0;
  const effort = values.timeTrackings.reduce((result, entry) => result + entry.effort, 0);

  return (
    <styled.TimeLogSection className="multi-group">
      <Col>
        <Row alignItems="center">
          <Text
            width={FieldSize.Small}
            name="prep"
            label="Prep time (minutes)"
            value={prepTime}
            type="number"
            disabled={!userId}
            required={prepTimeRequired && !values.timeTrackings.some((entry) => entry.id === 0)}
            onChange={(e) => {
              const value = parseFloat(e.target.value);
              if (value > 0) {
                if (onPrepTimeChanged) {
                  onPrepTimeChanged(value.toString());
                }
              }
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
