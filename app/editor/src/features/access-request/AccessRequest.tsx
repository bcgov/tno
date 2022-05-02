import { FieldSize } from 'components/form';
import { FormikForm, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import React from 'react';
import { useApp } from 'store/hooks';
import { Button, ButtonVariant, Col, Row, useKeycloakWrapper } from 'tno-core';

import { accountTypeOptions } from './constants';
import * as styled from './styled';

export const AccessRequest: React.FC = (props) => {
  const keycloak = useKeycloakWrapper();
  const [{ userInfo }] = useApp();

  const register = {
    username: userInfo?.username,
    email: userInfo?.email,
    firstName: userInfo?.firstName,
    lastName: userInfo?.lastName,
  };

  return (
    <styled.AccessRequest>
      <h1>Welcome</h1>
      <p>Hello {keycloak.getDisplayName()},</p>
      <p>
        If this is the first time signing into Today's News Online (TNO), you will need to request
        approval before using this site.
      </p>
      <Row wrap="nowrap" gap="2em">
        <Col className="preApproval">
          <h2>Preapproved</h2>
          <p>
            If you had a prior registered account with TNO, enter your prior email address and click
            'Send Code'. If your prior email address matches an approved account, we'll send you an
            email with a code.
          </p>
          <FormikForm
            initialValues={{}}
            onSubmit={(values, { setSubmitting }) => {
              console.debug(values);
              setSubmitting(false);
            }}
          >
            {({ isSubmitting }) => (
              <>
                <Row alignItems="end">
                  <FormikText name="email" label="Email Address" width={FieldSize.Big} />
                  <Button variant={ButtonVariant.secondary}>Send Code</Button>
                </Row>
                <p>Copy the code sent the above email address here.</p>
                <Row alignItems="end">
                  <FormikText name="code" label="Code" width={FieldSize.Small} />
                  <Button disabled={isSubmitting}>Validate</Button>
                </Row>
              </>
            )}
          </FormikForm>
        </Col>
        <Col className="register">
          <h2>Register New Account</h2>
          <p>
            If you are a new user, fill out the following form to register. You will receive an
            email confirmation once your account has been approved.
          </p>
          <FormikForm
            initialValues={register}
            onSubmit={(values, { setSubmitting }) => {
              console.debug(values);
              setSubmitting(false);
            }}
          >
            {({ isSubmitting }) => (
              <>
                <Row>
                  <FormikText name="username" label="Username" disabled width={FieldSize.Big} />
                  <FormikText name="email" label="Email Address" disabled width={FieldSize.Big} />
                </Row>
                <Row>
                  <FormikText name="firstName" label="First Name" disabled width={FieldSize.Big} />
                  <FormikText name="lastName" label="Last Name" disabled width={FieldSize.Big} />
                </Row>
                <Row>
                  <FormikSelect
                    name="accountType"
                    label="Account Type"
                    options={accountTypeOptions}
                    placeholder="Select one"
                    width={FieldSize.Big}
                  ></FormikSelect>
                </Row>
                <FormikTextArea name="message" label="Message"></FormikTextArea>
                <Row alignItems="flex-end" alignContent="flex-end">
                  <Button disabled={isSubmitting}>Register</Button>
                </Row>
              </>
            )}
          </FormikForm>
        </Col>
      </Row>
    </styled.AccessRequest>
  );
};
