import { Button } from 'components/button';
import React from 'react';
import { toast } from 'react-toastify';
import { useLookup, useUsers } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Checkbox, getDistinct, ISubscriberUserModel, Row } from 'tno-core';

import * as styled from './styled';

export const MyMinisterSettings: React.FC = () => {
  const [{ ministers }] = useLookup();
  const { updateUser } = useUsers();
  const [{ profile, impersonate }, { storeMyProfile, storeImpersonate }] = useProfileStore();

  const [activeMinisters, setActiveMinisters] = React.useState(ministers);

  const myMinisters: number[] = !!impersonate
    ? impersonate?.preferences?.myMinisters ?? []
    : profile?.preferences?.myMinisters ?? [];

  const mergeValues = React.useCallback(
    (values: number[]) => {
      // Remove inactive ministers;
      const result = activeMinisters.filter((m) => values.includes(m.id)).map((m) => m.id);
      return getDistinct([...result], (v) => v);
    },
    [activeMinisters],
  );

  const handleSubmit = React.useCallback(
    async (values: number[]) => {
      if (!profile) {
        toast.error('User information is missing. Please try again later');
        return;
      }
      const baseProfile = impersonate ?? profile;
      console.log('baseProfile', baseProfile);
      const createUser = (): ISubscriberUserModel => {
        // use impersonate if it exists, otherwise use profile
        return {
          ...baseProfile,
          preferences: {
            ...baseProfile.preferences,
            myMinisters: mergeValues(values),
          },
        };
      };

      const user = createUser();

      try {
        await updateUser(user, !!impersonate);
        toast.success('Your minister(s) have successfully been updated.');
      } catch (error) {
        // Handle the error, if needed
        console.error('Failed to update user:', error);
      }
    },
    [profile, impersonate, mergeValues, updateUser],
  );

  React.useEffect(() => {
    if (ministers.length > 0) {
      // Only display members who are active.
      setActiveMinisters(
        ministers
          .filter((m) => m.isEnabled)
          .sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name)),
      );
    }
  }, [ministers]);

  return (
    <styled.MyMinisterSettings>
      <p className="description">
        Choose the Minister(s) you'd like to follow. Stories about your selected Minister(s) will be
        available from a quick click in the sidebar menu.
      </p>
      <div className="option-container">
        <Row justifyContent="flex-end">
          <Button type="submit" onClick={() => handleSubmit(myMinisters)}>
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
                  const baseProfile = impersonate ?? profile;
                  if (baseProfile) {
                    const values = e.target.checked
                      ? [...myMinisters, o.id]
                      : myMinisters.filter((m) => m !== o.id);
                    const user = {
                      ...baseProfile,
                      preferences: {
                        ...baseProfile?.preferences,
                        myMinisters: values,
                      },
                    };
                    !!impersonate ? storeImpersonate(user) : storeMyProfile(user);
                  }
                }}
              />
              <div className="position">{o.position}</div>
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
