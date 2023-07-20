export interface IEveningOverviewItemType {
  [key: string]: {
    id: number;
    name: string;
  };
}
export const EveningOverviewItemType: IEveningOverviewItemType = {
  Intro: { id: 0, name: 'Intro' },
  Story: { id: 1, name: 'Story' },
  Ad: { id: 2, name: 'Ad' },
};
