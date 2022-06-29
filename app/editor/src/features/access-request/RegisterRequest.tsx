import { FormikForm, FormikText, FormikTextArea } from 'components/formik';
import { IUserModel } from 'hooks/api-editor';
import React from 'react';
import { toast } from 'react-toastify';
import { useUsers } from 'store/hooks';
import { Button, Col, FieldSize, Row } from 'tno-core';

export interface IRegisterRequestProps {
  user: IUserModel;
  setUser: (user: IUserModel) => void;
}

export const RegisterRequest: React.FC<IRegisterRequestProps> = ({ user, setUser }) => {
  const users = useUsers();

  const handleSubmit = async (values: IUserModel) => {
    try {
      const result = await users.requestApproval(values);
      setUser(result);
      toast.success(`Account request has been submitted.`);
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
        initialValues={user}
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
