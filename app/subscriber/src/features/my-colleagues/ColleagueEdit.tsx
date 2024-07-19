import { Button } from 'components/button';
import { FormikForm } from 'components/formik';
import { PageSection } from 'components/section';
import React from 'react';
import { FaBackward, FaSave } from 'react-icons/fa';
import { useColleagues } from 'store/hooks';
import { Col, FormikText, validateEmail } from 'tno-core';

import { ColleagueActionEnum } from './constants/ColleagueActionEnum';
import { defaultColleague } from './constants/defaultColleague';
import { IColleagueForm } from './interfaces/IColleagueForm';
import { IMyColleaguesProps } from './interfaces/IMyColleaguesProps';
import * as styled from './styled';

export const ColleagueEdit: React.FC<IMyColleaguesProps> = ({ changeAction }) => {
  const [{ addColleague }] = useColleagues();
  const [colleague] = React.useState<IColleagueForm>(defaultColleague);

  const handleSubmit = React.useCallback(
    async (values: IColleagueForm) => {
      try {
        await addColleague(values.colleagueEmail);
        changeAction(ColleagueActionEnum.List);
      } catch {}
    },
    [addColleague, changeAction],
  );

  return (
    <styled.MyColleagues>
      <PageSection header="Add Colleague">
        <FormikForm
          loading={false}
          initialValues={colleague}
          onSubmit={async (values, { setSubmitting }) => {
            await handleSubmit(values);
            setSubmitting(false);
          }}
        >
          {({ values, isSubmitting, setFieldValue, submitForm }) => (
            <>
              <div
                className="back"
                onClick={() => {
                  changeAction(ColleagueActionEnum.List);
                }}
              >
                <FaBackward /> Back
              </div>
              <Col className="edit">
                <FormikText
                  name={`colleagueEmail`}
                  label="Colleague Email"
                  onChange={(e) => {
                    setFieldValue(`colleagueEmail`, e.target.value);
                  }}
                >
                  <Button
                    variant="primary"
                    disabled={isSubmitting || !validateEmail(values.colleagueEmail)}
                    onClick={() => submitForm()}
                  >
                    <FaSave />
                  </Button>
                </FormikText>
              </Col>
            </>
          )}
        </FormikForm>
      </PageSection>
    </styled.MyColleagues>
  );
};
