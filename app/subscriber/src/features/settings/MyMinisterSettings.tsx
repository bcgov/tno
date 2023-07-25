import React from 'react';
import { toast } from 'react-toastify';
import { useApp, useLookup, useUsers } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Button, Checkbox, IUserInfoModel, IUserModel, Row } from 'tno-core';

import * as styled from './styled';

export const MyMinisterSettings: React.FC = () => {
  const [{ ministers }] = useLookup();
  const [{ userInfo }] = useApp();
  const [myMinisters, setMyMinisters] = React.useState<number[]>([]);
  const [, store] = useAppStore();
  const api = useUsers();

  let activeMinisters = ministers
    .filter((m) => m.isEnabled)
    .sort((a, b) => a.name.localeCompare(b.name));

  const handleSubmit = async (values: number[]) => {
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
    if (userInfo?.preferences?.myMinisters?.length > 0) {
      setMyMinisters(userInfo?.preferences?.myMinisters);
    }
  }, [userInfo]);

  React.useEffect(() => {
    // check if any of the users previous selections are no longer active
    if (userInfo?.preferences?.myMinisters?.length > 0 && activeMinisters.length > 0) {
      let activeSelectedMinisters: number[] = [];
      let inactiveSelectedMinisters: number[] = [];
      userInfo?.preferences?.myMinisters.forEach((m: number) => {
        const isActive = activeMinisters.find((element) => element.id === m);
        if (isActive) activeSelectedMinisters.push(m);
        else inactiveSelectedMinisters.push(m);
      });
      if (inactiveSelectedMinisters.length !== 0) {
        const user = {
          ...userInfo,
          preferences: { ...userInfo?.preferences, myMinisters: activeSelectedMinisters },
        } as IUserModel;
        api.updateUser(user, userInfo?.id ?? 0);
        toast.success(
          `Due to the following minister(s) no longer being active, ` +
            `your selection has been updated automatically: ` +
            `${inactiveSelectedMinisters.join(', ')}`,
        );
        store.storeUserInfo(user as IUserInfoModel);
      }
    }
  }, [activeMinisters, api, store, userInfo]);

  return (
    <styled.MyMinisterSettings>
      <p className="description">
        Choose the Minister(s) you'd like to follow. Stories about your selected Minister(s) will be
        available from a quick click in the sidebar menu.
      </p>
      <p>Please note that "Search Aliases" will also show up in your "My Minister" feed.</p>
      <div className="option-container">
        {activeMinisters.map((o) => {
          return (
            <div className="chk-container" key={o.id}>
              <Checkbox
                label={`${o.name} : ${o.position}`}
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
              <div className="aliases">Search Aliases: {o.aliases}</div>
            </div>
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
