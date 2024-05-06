import { Button } from 'components/button';
import { FormikForm } from 'components/formik';
import { PageSection } from 'components/section';
import React from 'react';
import { FaSave } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { useColleagues } from 'store/hooks';
import { Col, FormikText, validateEmail } from 'tno-core';

import { defaultColleague } from './constants/defaultColleague';
import { IColleagueForm } from './interfaces/IColleagueForm';
import * as styled from './styled';

export const ColleagueEdit: React.FC = () => {
  const navigate = useNavigate();
  const [{ addColleague }] = useColleagues();
  const [colleague] = React.useState<IColleagueForm>(defaultColleague);

  const handleSubmit = React.useCallback(
    async (values: IColleagueForm) => {
      try {
        await addColleague(values.colleagueEmail);
        navigate('/colleagues');
      } catch {}
    },
    [addColleague, navigate],
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
