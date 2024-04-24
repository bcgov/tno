import React from 'react';
import { toast } from 'react-toastify';
import { useApp, useLookup, useUsers } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Button, Checkbox, ISubscriberUserModel, IUserInfoModel, IUserModel, Row } from 'tno-core';

import * as styled from './styled';

export const MyMinisterSettings: React.FC = () => {
  const [{ ministers }] = useLookup();
  const [{ userInfo }] = useApp();
  const [myMinisters, setMyMinisters] = React.useState<number[]>([]);
  const [, store] = useAppStore();
  const api = useUsers();

  let activeMinisters = ministers
    .filter((m) => m.isEnabled)
    .sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name));

  const handleSubmit = async (values: number[], userInfo: IUserInfoModel) => {
    if (!userInfo) {
      toast.error('User information is missing. Please try again later');
      return;
    }

    try {
      var user = {
        ...userInfo,
        preferences: { ...userInfo.preferences, myMinisters: values },
      } as ISubscriberUserModel;
      user = await api.updateUser(user, userInfo.id);
      toast.success(`Your minister(s) have successfully been updated.`);
      store.storeUserInfo({ ...userInfo, preferences: user.preferences });
    } catch {}
  };

  React.useEffect(() => {
    if (userInfo?.preferences?.myMinisters?.length > 0) {
      setMyMinisters(userInfo?.preferences?.myMinisters);
    }
  }, [userInfo]);

  React.useEffect(() => {
    // check if any of the users previous selections are no longer active
    if (userInfo && userInfo?.preferences?.myMinisters?.length > 0 && activeMinisters.length > 0) {
      let activeSelectedMinisters: number[] = [];
      let inactiveSelectedMinisters: number[] = [];
      userInfo.preferences?.myMinisters.forEach((m: number) => {
        const isActive = activeMinisters.find((element) => element.id === m);
        if (isActive) activeSelectedMinisters.push(m);
        else inactiveSelectedMinisters.push(m);
      });
      if (inactiveSelectedMinisters.length !== 0) {
        var user = {
          ...userInfo,
          preferredEmail: '',
          preferences: { ...userInfo.preferences, myMinisters: activeSelectedMinisters },
          isSystemAccount: false,
          emailVerified: false,
          uniqueLogins: 0,
        } as IUserModel;
        api
          .updateUser(user, userInfo.id)
          .then((user) => {
            toast.success(
              'One of more of your selected ministers are no longer enabled. ' +
                'Your selection has been updated automatically: ',
            );
            store.storeUserInfo({ ...userInfo, preferences: user.preferences });
          })
          .catch(() => {});
      }
    }
  }, [activeMinisters, api, store, userInfo]);

  return (
    <styled.MyMinisterSettings>
      <p className="description">
        Choose the Minister(s) you'd like to follow. Stories about your selected Minister(s) will be
        available from a quick click in the sidebar menu.
      </p>
      <div className="option-container">
        <Row justifyContent="flex-end">
          <Button type="submit" onClick={() => handleSubmit(myMinisters, userInfo!)}>
            Save
          </Button>
        </Row>
        {activeMinisters.map((o) => {
          return (
            <div className="chk-container" key={o.id}>
              <Checkbox
                name={`minister-${o.id}`}
                label={`${o.name} : `}
                checked={myMinisters.includes(o.id)}
                onChange={(e) => {
                  if ((e.target as HTMLInputElement).checked) {
                    setMyMinisters([...myMinisters, o.id]);
                  } else {
                    // remove from preferences when unchecking
                    setMyMinisters(myMinisters.filter((m: number) => m !== o.id));
                  }
                }}
              />
              <div className="position">{o.position}</div>
            </div>
          );
        })}

        <Row justifyContent="flex-end">
          <Button type="submit" onClick={() => handleSubmit(myMinisters, userInfo!)}>
            Save
          </Button>
        </Row>
      </div>
    </styled.MyMinisterSettings>
  );
};
