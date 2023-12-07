import { FormikForm } from 'components/formik';
import { Header } from 'components/header';
import { PageSection } from 'components/section';
import React from 'react';
import { FaSave } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { useColleagues } from 'store/hooks';
import { Button, ButtonVariant, Col, Container, FormikText, IColleagueModel, Row } from 'tno-core';

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
        const model: IColleagueModel = {
          colleagueId: undefined,
          userId: undefined,
          colleague: {
            id: undefined,
            username: undefined,
            email: values.colleagueEmail,
          },
        };
        await addColleague(model);
        navigate('/colleagues');
      } catch {}
    },
    [addColleague, navigate],
  );

  return (
    <styled.MyColleagues>
      <Header />
      <PageSection header="Add Colleague">
        <FormikForm
          loading={false}
          initialValues={colleague}
          onSubmit={async (values, { setSubmitting }) => {
            await handleSubmit(values);
            setSubmitting(false);
          }}
        >
          {({ isSubmitting, setFieldValue, submitForm }) => (
            <>
              <Row gap="1rem">
                <Col className="edit">
                  <Container>
                    <FormikText
                      name={`colleagueEmail`}
                      label="Colleague Email"
                      onChange={(e) => {
                        setFieldValue(`colleagueEmail`, e.target.value);
                      }}
                    />
                  </Container>
                </Col>
              </Row>
              <Row justifyContent="flex-end">
                <Button
                  variant={ButtonVariant.success}
                  disabled={isSubmitting}
                  title="Save changes"
                  onClick={() => submitForm()}
                >
                  <FaSave />
                </Button>
              </Row>
            </>
          )}
        </FormikForm>
      </PageSection>
    </styled.MyColleagues>
  );
};
