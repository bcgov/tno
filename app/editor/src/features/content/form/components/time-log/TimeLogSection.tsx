import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight, FaRegListAlt } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  FieldSize,
  FormikText,
  IUserModel,
  Modal,
  Row,
  Text,
  useKeycloakWrapper,
  useModal,
} from 'tno-core';

import { TimeLogTable } from '../../TimeLogTable';
import { getTotalTime } from '../../utils';
import * as styled from './styled';

export interface ITimeLogSectionProps {
  prepTimeRequired?: boolean;
}

/**
 * TimeLogSection contains the input for time tracking gor a content item.
 */
export const TimeLogSection: React.FC<ITimeLogSectionProps> = ({ prepTimeRequired = false }) => {
  const keycloak = useKeycloakWrapper();
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const { isShowing, toggle } = useModal();
  const [{ users }] = useLookup();

  const [effort, setEffort] = React.useState(0);
  const [prep, setPrep] = React.useState<number | ''>('');

  const userId = users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id;

  React.useEffect(() => {
    const value = getTotalTime(values.timeTrackings ?? []);
    setEffort(value);
    setFieldValue('efforts', value);
  }, [setFieldValue, values.timeTrackings]);

  const addTime = React.useCallback(
    (value: number | string) => {
      if (!!values.timeTrackings && typeof value === 'number' && value > 0) {
        setFieldValue('timeTrackings', [
          ...values.timeTrackings,
          {
            userId: userId,
            activity: !!values.id ? 'Updated' : 'Created',
            effort: value,
            createdOn: new Date(),
          },
        ]);
        setPrep('');
      }
    },
    [setFieldValue, userId, values.id, values.timeTrackings],
  );

  return (
    <styled.TimeLogSection className="multi-group">
      <Text
        width={FieldSize.Small}
        name="prep"
        label="Prep time (minutes)"
        value={prep}
        type="number"
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
        <FormikText
          disabled
          className="total-mins"
          width={FieldSize.Small}
          name="efforts"
          label="Total minutes"
          value={effort?.toString()}
          required={prepTimeRequired && effort <= 0}
        />
        <FaRegListAlt
          className="action-button"
          onClick={() => {
            if (!!values.timeTrackings) setFieldValue('timeTrackings', values.timeTrackings);
            toggle();
          }}
        />
      </Row>
      <Modal
        hide={toggle}
        isShowing={isShowing}
        headerText="Prep Time Log"
        body={
          <TimeLogTable
            setTotalEffort={setEffort}
            totalEffort={effort}
            data={values.timeTrackings ?? []}
          />
        }
        customButtons={
          <Button variant={ButtonVariant.secondary} onClick={toggle}>
            Close
          </Button>
        }
      />
    </styled.TimeLogSection>
  );
};
