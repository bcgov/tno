export interface IHolidayModel {
  id: number;
  date: Date | string;
  nameEn: string;
  nameFr: string;
  federal: boolean;
  observedDate: Date | string;
}
