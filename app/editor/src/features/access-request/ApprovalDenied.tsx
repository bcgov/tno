import { FormikForm, FormikText, FormikTextArea } from 'components/formik';
import { IUserModel } from 'hooks/api-editor';
import React from 'react';
import { toast } from 'react-toastify';
import { useUsers } from 'store/hooks';
import { Button, Col, FieldSize, Row } from 'tno-core';

export interface IApprovalDeniedProps {
  user: IUserModel;
  setUser: (user: IUserModel) => void;
}

export const ApprovalDenied: React.FC<IApprovalDeniedProps> = ({ user, setUser }) => {
  const users = useUsers();

  const handleSubmit = async (values: IUserModel) => {
    try {
      const result = await users.requestApproval(values);
      setUser(result);
      toast.success(`Account request has been updated.`);
    } catch {}
  };

  return (
    <Col className="status">
      <h2>Approval Denied</h2>
      <p>Your request has been denied.</p>
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
                Update
              </Button>
            </Row>
          </>
        )}
      </FormikForm>
    </Col>
  );
};
