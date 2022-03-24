import { Row } from 'components/flex';
import { FieldSize } from 'components/form';
import { FormikText } from 'components/formik';
import React from 'react';

import * as styled from './styled';

interface IReachEarnedMediaProps {}

export const ReachEarnedMedia: React.FC<IReachEarnedMediaProps> = () => {
  return (
    <styled.ReachEarnedMedia className="reach-earned-media" flex="1">
      <h2>Reach / Earned Media</h2>
      <Row nowrap>
        <div className="rem-label">
          <label>Monday</label>
        </div>
        <FormikText
          label="Reach"
          name="connection.reach"
          width={FieldSize.Small}
          type="number"
          min={0}
          max={10}
        />
        <FormikText label="Earned Media" name="" width={FieldSize.Small} type="number" />
      </Row>
      <Row nowrap>
        <div className="rem-label">
          <label>Tuesday</label>
        </div>
        <FormikText label="Reach" name="" width={FieldSize.Small} type="number" />
        <FormikText label="Earned Media" name="" width={FieldSize.Small} type="number" />
      </Row>
      <Row nowrap>
        <div className="rem-label">
          <label>Wednesday</label>
        </div>
        <FormikText label="Reach" name="" width={FieldSize.Small} type="number" />
        <FormikText label="Earned Media" name="" width={FieldSize.Small} type="number" />
      </Row>
      <Row nowrap>
        <div className="rem-label">
          <label>Thursday</label>
        </div>
        <FormikText label="Reach" name="" width={FieldSize.Small} type="number" />
        <FormikText label="Earned Media" name="" width={FieldSize.Small} type="number" />
      </Row>
      <Row nowrap>
        <div className="rem-label">
          <label>Friday</label>
        </div>
        <FormikText label="Reach" name="" width={FieldSize.Small} type="number" />
        <FormikText label="Earned Media" name="" width={FieldSize.Small} type="number" />
      </Row>
      <Row nowrap>
        <div className="rem-label">
          <label>Saturday</label>
        </div>
        <FormikText label="Reach" name="" width={FieldSize.Small} type="number" />
        <FormikText label="Earned Media" name="" width={FieldSize.Small} type="number" />
      </Row>
      <Row nowrap>
        <div className="rem-label">
          <label>Sunday</label>
        </div>
        <FormikText label="Reach" name="" width={FieldSize.Small} type="number" />
        <FormikText label="Earned Media" name="" width={FieldSize.Small} type="number" />
      </Row>
    </styled.ReachEarnedMedia>
  );
};
