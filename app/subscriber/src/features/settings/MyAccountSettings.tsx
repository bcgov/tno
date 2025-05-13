import React from 'react';
import { FaEnvelope, FaSmile, FaToggleOff, FaToggleOn } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useUsers } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { ISubscriberUserModel, ToggleButton } from 'tno-core';

import * as styled from './styled';

export const toggleVacationMode = async (
    profile: ISubscriberUserModel | undefined,
    impersonate: ISubscriberUserModel | undefined,
    isVacationMode: boolean,
    updateUser: (model: ISubscriberUserModel, impersonate?: boolean) => Promise<ISubscriberUserModel>,
  ) => {
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
          isVacationMode: isVacationMode,
        },
      };
    };
    const user = createUser();

    try {
      await updateUser(user, !!impersonate);
      toast.success('Vacation mode has successfully been updated.');
    } catch (error) {
      // Handle the error, if needed
      console.error('Failed to update user:', error);
    }
  };

const MyAccountSettings = () => {
  const { updateUser } = useUsers();
  const [{ profile, impersonate }] = useProfileStore();
  const isVacationMode: boolean = !!impersonate
    ? impersonate?.preferences?.isVacationMode ?? false
    : profile?.preferences?.isVacationMode ?? false;
  const enableReportSentiment: boolean = !!impersonate
    ? impersonate?.preferences?.enableReportSentiment ?? false
    : profile?.preferences?.enableReportSentiment ?? false;

  
  const toggleReportSentiment = React.useCallback(
    async (
      profile: ISubscriberUserModel | undefined,
      impersonate: ISubscriberUserModel | undefined,
      enableReportSentiment: boolean,
    ) => {
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
            enableReportSentiment: enableReportSentiment,
          },
        };
      };
      const user = createUser();

      try {
        await updateUser(user, !!impersonate);
        toast.success('Report sentiment has successfully been updated.');
      } catch (error) {
        // Handle the error, if needed
        console.error('Failed to update user:', error);
      }
    },
    [updateUser],
  );

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
        <span className="vacation-mode-label">Vacation Mode</span>
        <ToggleButton
          on={<FaToggleOn />}
          off={<FaToggleOff />}
          onClick={() => toggleVacationMode(profile, impersonate, !isVacationMode, updateUser)}
          width="25px"
          height="25px"
          label=""
          value={isVacationMode}
        />
      </div>
      <div className="header-row">
        <FaSmile className="icon" />
        <span className="header-text">Report sentiment</span>
      </div>
      <p className="description">
        Enables display of sentiment in the reports you receive. Sentiment reflects the tone of
        media articles towards government and appears in reports and alerts as emoticons.
      </p>
      <div className="toggleContainer">
        <span className="vacation-mode-label">Report sentiment</span>
        <ToggleButton
          on={<FaToggleOn />}
          off={<FaToggleOff />}
          onClick={() => toggleReportSentiment(profile, impersonate, !enableReportSentiment)}
          width="25px"
          height="25px"
          label=""
          value={enableReportSentiment}
        />
      </div>
    </styled.MyAccountSettings>
  );
};

export default MyAccountSettings;
