import { FormikForm } from 'components/formik';
import { IUserModel } from 'hooks/api-editor';
import React from 'react';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { Button, Col, FieldSize, FormikText, FormikTextArea, Row } from 'tno-core';

import { toUserModel } from './utils';

export const ApprovalStatus: React.FC = () => {
  const [{ userInfo }, { getUserInfo, requestApproval }] = useApp();

  const handleSubmit = async (values: IUserModel) => {
    try {
      await requestApproval(values);
      await getUserInfo(true);
      toast.success(`Account request has been updated.`);
    } catch {}
  };

  return (
    <Col className="status">
      <h2>Approval Status</h2>
      <p>
        Your request has been successfully submitted. Please wait for email confirmation to gain
        access.
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
                Update
              </Button>
            </Row>
          </>
        )}
      </FormikForm>
    </Col>
  );
};
