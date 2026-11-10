import { FormikForm } from 'components/formik';
import React from 'react';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { Button, Col, FieldSize, FormikText, FormikTextArea, IUserModel, Row } from 'tno-core';

import { toUserModel } from './utils';

export const RegisterRequest: React.FC = () => {
  const [{ userInfo }, { getUserInfo, requestApproval }] = useApp();

  const handleSubmit = async (values: IUserModel) => {
    try {
      await requestApproval(values);
      await getUserInfo(true);
      toast.success(`Account request has been updated.`);
    } catch {}
  };

  return (
    <Col className="register">
      <h2>New Account</h2>
      <p>
        If you are a new user, fill out the following form to register. You will receive an email
        confirmation once your account has been approved.
      </p>
      <FormikForm
        initialValues={toUserModel(userInfo)}
        onSubmit={async (values, { setSubmitting }) => {
          await handleSubmit(values);
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
            <FormikTextArea name="note" label="Message"></FormikTextArea>
            <Row alignItems="flex-end" alignContent="flex-end">
              <Button type="submit" disabled={isSubmitting}>
                Register
              </Button>
            </Row>
          </>
        )}
      </FormikForm>
    </Col>
  );
};
