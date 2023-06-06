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

  const userId = users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id;

  React.useEffect(() => {
    const value = getTotalTime(values.timeTrackings ?? []);
    setEffort(value);
    setFieldValue('efforts', value);
  }, [setFieldValue, values.timeTrackings]);

  return (
    <styled.TimeLogSection>
      <FormikText
        width={FieldSize.Small}
        name="prep"
        label="Prep time (minutes)"
        type="number"
        required={prepTimeRequired && effort <= 0}
      />
      <FaArrowAltCircleRight
        className="action-button"
        onClick={() => {
          if (!!values.timeTrackings) {
            const value = effort!! + Number((values as any).prep);
            setEffort(value);
            setFieldValue('timeTrackings', [
              ...values.timeTrackings,
              {
                userId: userId,
                activity: !!values.id ? 'Updated' : 'Created',
                effort: (values as any).prep,
                createdOn: new Date(),
              },
            ]);
            setFieldValue('prep', '');
            setFieldValue('efforts', value);
          }
        }}
      />
      <Row className="disabled-section">
        <FormikText
          disabled
          className="total-mins"
          width={FieldSize.Small}
          name="total"
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
