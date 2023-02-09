import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import { IContentModel } from 'hooks';
import React from 'react';
import { FaArrowAltCircleRight, FaRegListAlt } from 'react-icons/fa';
import { FieldSize, FormikText, Row } from 'tno-core';

import * as styled from './styled';

export interface ITimeLogSectionProps {
  toggle: () => void;
  setEffort: React.Dispatch<React.SetStateAction<number>>;
  effort: number;
  setContent: (content: IContentForm) => void;
  content: IContentForm;
  userId: number;
}

export const TimeLogSection: React.FC<ITimeLogSectionProps> = ({
  effort,
  setEffort,
  toggle,
  content,
  setContent,
  userId,
}) => {
  const { values, setFieldValue } = useFormikContext<IContentModel>();
  return (
    <styled.TimeLogSection>
      <FormikText width={FieldSize.Small} name="prep" label="Prep time (minutes)" type="number" />
      <FaArrowAltCircleRight
        className="action-button"
        onClick={() => {
          if (!!values.timeTrackings) {
            setEffort(effort!! + Number((values as any).prep));
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
        />
        <FaRegListAlt
          className="action-button"
          onClick={() => {
            if (!!values.timeTrackings)
              setContent({ ...content, timeTrackings: values.timeTrackings });
            toggle();
          }}
        />
      </Row>
    </styled.TimeLogSection>
  );
};
