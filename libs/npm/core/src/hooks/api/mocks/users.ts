import { IUserModel } from '..';
import { UserAccountTypeName, UserStatusName } from '../constants';

export const mockUsers: IUserModel[] = [
  {
    id: 1,
    key: '03132449-c183-4341-b42a-2f0ec851d8dc',
    username: 'username',
    email: 'email',
    preferredEmail: '',
    displayName: 'user',
    firstName: 'first',
    lastName: 'last',
    isEnabled: true,
    status: UserStatusName.Preapproved,
    isSystemAccount: false,
    accountType: UserAccountTypeName.Direct,
    emailVerified: false,
    preferences: {},
    note: '',
    uniqueLogins: 0,
  },
];
