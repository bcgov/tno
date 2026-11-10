import { Claim } from '@/shared';

export interface INavbarItem {
  label: string;
  icon: JSX.Element;
  path: string;
  groupName?: string;
  secondaryIcon?: JSX.Element;
  secondaryIconRoute?: string;
  claims?: Claim[];
}
