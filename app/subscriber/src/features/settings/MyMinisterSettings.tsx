import { FormikForm } from 'components/formik';
import { defaultUser } from 'features/access-request/constants';
import React from 'react';
import { toast } from 'react-toastify';
import { useApp, useLookup, useUsers } from 'store/hooks';
import { Button, IOptionItem, IUserModel, RadioGroup, Row } from 'tno-core';

import * as styled from './styled';

export const MyMinisterSettings: React.FC = () => {
  const [{ ministers }] = useLookup();
  const [myMinister, setMyMinister] = React.useState<string>();
  const [{ userInfo }] = useApp();
  const api = useUsers();

  const [user, setUser] = React.useState<IUserModel>(defaultUser);

  React.useEffect(() => {
    if (userInfo && userInfo.id) {
      api.getUser(userInfo.id).then((data) => {
        setUser(data);
      });
    }
  }, [userInfo, api]);

  React.useEffect(() => {
    if (!!myMinister) localStorage.setItem('myMinister', myMinister);
  }, [myMinister]);

  const options = ministers.map((m) => {
    return {
      label: (
        <Row className="options">
          <b>{m.name} | </b>
          <span className="desc">{m.description}</span>
        </Row>
      ),
      value: m.name,
      isEnabled: true,
      discriminator: 'IOption',
    } as IOptionItem;
  });

  const handleSubmit = async (values: IUserModel) => {
    try {
      await api.updateUser(values);
      toast.success(
        `${values.preferences.myMinister} has successfully been chosen as your minister.`,
      );
    } catch {}
  };

  return (
    <styled.MyMinisterSettings>
      <p className="description">
        Choose the Minister you'd like to follow. Stories about your selected Minister will be
        available from a quick click in the sidebar menu.
      </p>
      <FormikForm
        initialValues={{ ...user, preferences: { myMinister: myMinister } }}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ values, setFieldValue }) => (
          <>
            <RadioGroup
              value={
                !!values.preferences.myMinister
                  ? options.find((o) => o.value === values.preferences.myMinister)
                  : options.find((o) => o.value === user.preferences?.myMinister)
              }
              onChange={(e) => {
                setMyMinister(e.target.value);
                setFieldValue('preferences.myMinister', e.target.value);
              }}
              options={options}
              name="ministers"
            />
            <Button type="submit">SAVE</Button>
          </>
        )}
      </FormikForm>
    </styled.MyMinisterSettings>
  );
};
