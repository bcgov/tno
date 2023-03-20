import { FormikForm } from 'components/formik';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikText,
  IRegisterModel,
  Row,
  UserStatusName,
} from 'tno-core';

export const PreapprovedRequest: React.FC = () => {
  const [, app] = useApp();
  const navigate = useNavigate();

  const handleRequestCode = async (values: IRegisterModel) => {
    try {
      const result = await app.requestCode({ ...values, code: '' });
      if (result.status === UserStatusName.Approved)
        toast.success(`Account request has been submitted.`);
      else toast.error(result.message);
    } catch {}
  };

  const handleValidateCode = async (values: IRegisterModel) => {
    try {
      const result = await app.requestCode(values);
      if (result.status === UserStatusName.Approved) {
        const userInfo = await app.getUserInfo(true);
        if (!!userInfo.roles.length) navigate('/');
      } else {
        toast.error(result.message);
      }
    } catch {}
  };

  const register: IRegisterModel = {
    email: '',
    code: '',
    status: UserStatusName.Preapproved,
    message: '',
  };

  return (
    <Col className="preApproval">
      <h2>Existing Account</h2>
      <p>
        If you had a prior registered account with Media Monitoring Insights & Analysis, enter your
        registered email address and click 'Send Code'. If your prior email address matches an
        approved account, you will receive an email with a code.
      </p>
      <FormikForm
        initialValues={register}
        onSubmit={(values, { setSubmitting }) => {
          handleRequestCode(values);
          setSubmitting(false);
        }}
      >
        {({ values, isSubmitting, setSubmitting }) => (
          <>
            <Row alignItems="end">
              <FormikText name="email" label="Email Address" width={FieldSize.Big} required />
              <Button type="submit" disabled={isSubmitting}>
                Send Code
              </Button>
            </Row>
            <p>Paste the code sent to the above email address here.</p>
            <Row alignItems="end">
              <FormikText name="code" label="Code" width={FieldSize.Small} />
              <Button
                variant={ButtonVariant.secondary}
                disabled={isSubmitting}
                onClick={async () => {
                  setSubmitting(true);
                  await handleValidateCode(values);
                  setSubmitting(false);
                }}
              >
                Validate
              </Button>
            </Row>
          </>
        )}
      </FormikForm>
    </Col>
  );
};
