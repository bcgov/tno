import React from 'react';
import { toast } from 'react-toastify';
import { useApp, useLookup, useUsers } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Button, Checkbox, IUserInfoModel, IUserModel, OptionItem, Row } from 'tno-core';

import * as styled from './styled';

export const MyMinisterSettings: React.FC = () => {
  const [{ ministers }] = useLookup();
  const [{ userInfo }] = useApp();
  const [myMinisters, setMyMinisters] = React.useState<string[]>([]);
  const [, store] = useAppStore();
  const api = useUsers();

  const options = ministers.map((m) => new OptionItem(`${m.name} | ${m.description}`, m.name));

  const handleSubmit = async (values: string[]) => {
    try {
      const user = {
        ...userInfo,
        preferences: { ...userInfo?.preferences, myMinisters: values },
      } as IUserModel;
      await api.updateUser(user, userInfo?.id ?? 0);
      toast.success(`Your minister(s) have succesfully been updated.`);
      store.storeUserInfo(user as IUserInfoModel);
    } catch {}
  };

  React.useEffect(() => {
    if (userInfo?.preferences?.myMinisters) {
      setMyMinisters(userInfo?.preferences?.myMinisters);
    }
  }, [userInfo, ministers]);

  return (
    <styled.MyMinisterSettings>
      <p className="description">
        Choose the Minister you'd like to follow. Stories about your selected Minister will be
        available from a quick click in the sidebar menu.
      </p>
      <div className="option-container">
        {options.map((o) => {
          return (
            <Checkbox
              key={o.value}
              label={o.label}
              checked={myMinisters.includes(o.value)}
              onChange={(e) => {
                if ((e.target as HTMLInputElement).checked) {
                  setMyMinisters([...myMinisters, o.value]);
                } else {
                  // remove from prefrerences when unchecking
                  setMyMinisters(myMinisters.filter((m: string) => m !== o.value));
                }
              }}
            />
          );
        })}

        <Row justifyContent="flex-end">
          <Button type="submit" onClick={() => handleSubmit(myMinisters)}>
            Save
          </Button>
        </Row>
      </div>
    </styled.MyMinisterSettings>
  );
};
