import { IUserModel, UserStatusName } from 'hooks/api-editor';

export const defaultUser: IUserModel = {
  id: 0,
  key: '00000000-0000-0000-0000-000000000000',
  username: '',
  email: '',
  displayName: '',
  firstName: '',
  lastName: '',
  isEnabled: true,
  status: UserStatusName.Preapproved,
  emailVerified: false,
  isSystemAccount: false,
  note: '',
  roles: [],
};
