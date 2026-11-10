import { IUserInfoModel, IUserModel } from 'tno-core';

import { defaultUser } from '../constants';

export const toUserModel = (userInfo?: IUserInfoModel): IUserModel => {
  return {
    ...defaultUser,
    id: userInfo?.id ?? 0,
    key: userInfo?.key ?? '',
    username: userInfo?.username ?? '',
    email: userInfo?.email ?? '',
    displayName: userInfo?.displayName ?? '',
    firstName: userInfo?.firstName ?? '',
    lastName: userInfo?.lastName ?? '',
    note: userInfo?.note ?? '',
    isEnabled: userInfo?.isEnabled ?? defaultUser.isEnabled,
    status: userInfo?.status ?? defaultUser.status,
  };
};
