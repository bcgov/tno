export interface IUserFilter {
  username?: string;
  email?: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  isEnabled?: boolean;
  isSystemAccount?: boolean;
  status?: string;
  sort: any;
  role?: string;
  keyword?: string;
}
