import { IUserModel, UserAccountTypeName, UserStatusName } from 'tno-core';

export const defaultUser: IUserModel = {
  id: 0,
  key: '00000000-0000-0000-0000-000000000000',
  username: '',
  email: '',
  preferredEmail: '',
  displayName: '',
  firstName: '',
  lastName: '',
  isEnabled: true,
  status: UserStatusName.Activated,
  accountType: UserAccountTypeName.Direct,
  emailVerified: false,
  isSystemAccount: false,
  note: '',
  roles: [],
  uniqueLogins: 0,
};
