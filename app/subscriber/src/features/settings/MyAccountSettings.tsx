import React from 'react';
import { FaEnvelope, FaToggleOff, FaToggleOn, FaUmbrellaBeach } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useUsers } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { ISubscriberUserModel, ToggleButton } from 'tno-core';

import * as styled from './styled';

const MyAccountSettings = () => {
  const { updateUser } = useUsers();
  const [{ profile, impersonate }, { storeMyProfile, storeImpersonate }] = useProfileStore();
  const isVacationMode: boolean = !!impersonate
    ? impersonate?.preferences?.isVacationMode ?? false
    : profile?.preferences?.isVacationMode ?? false;

  const toggleVacationMode = React.useCallback(async () => {
    if (!profile) {
      toast.error('User information is missing. Please try again later');
      return;
    }
    const baseProfile = impersonate ?? profile;
    const createUser = (): ISubscriberUserModel => {
      // use impersonate if it exists, otherwise use profile
      return {
        ...baseProfile,
        preferences: {
          ...baseProfile.preferences,
          isVacationMode: !isVacationMode,
        },
      };
    };
    const user = createUser();

    try {
      !!impersonate ? storeImpersonate(user) : storeMyProfile(user);
      await updateUser(user, !!impersonate);
      toast.success('Vacation mode has successfully been updated.');
    } catch (error) {
      // Handle the error, if needed
      console.error('Failed to update user:', error);
    }
  }, [profile, impersonate, isVacationMode, storeImpersonate, storeMyProfile, updateUser]);

  return (
    <styled.MyAccountSettings>
      <div className="header-row">
        <FaEnvelope className="icon" />
        <span className="header-text">E-mail notifications</span>
      </div>
      <p className="description">
        Enabling vacation mode will <strong>turn off</strong> all MMI emails to you, until you
        disable vacation mode. This will include subscriptions to MMI Products, Alerts and Reports.
      </p>
      <div className="toggleContainer">
        <ToggleButton
          on={<FaToggleOn />}
          off={<FaToggleOff />}
          onClick={toggleVacationMode}
          width="25px"
          height="25px"
          color="#008000"
          label={
            <span className="vacation-mode-label">
              <FaUmbrellaBeach className="icon" />
              Vacation Mode
            </span>
          }
          value={isVacationMode}
        />
      </div>
    </styled.MyAccountSettings>
  );
};

export default MyAccountSettings;
