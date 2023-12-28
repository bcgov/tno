export interface IUserLocationModel {
  key: string;
  IPv4: string;
  country_code: string;
  country_name: string;
  city: string;
  state: string;
  postal: string;
  latitude: number;
  longitude: number;
  lastLoginOn?: string;
}
