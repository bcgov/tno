import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowAltCircleRight, FaRegListAlt } from 'react-icons/fa';
import { FieldSize, FormikText, IContentModel, Row } from 'tno-core';

import * as styled from './styled';

export interface ITimeLogSectionProps {
  toggle: () => void;
  setEffort: React.Dispatch<React.SetStateAction<number>>;
  effort: number;
  setContent: (content: IContentForm) => void;
  content: IContentForm;
  userId: number;
}

/**
 * TimeLogSection contains the input for time tracking gor a content item.
 * @param effort The total effort for the content item.
 * @param setEffort The setter for the total effort.
 * @param toggle The toggle function for the time log modal.
 * @param content The content from parent.
 * @param setContent The setter for the content.
 * @param userId The id of the user.
 */
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
