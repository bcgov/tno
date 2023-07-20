import { OptionItem } from 'tno-core';

export enum EveningOverviewItemType {
  Intro = 'Intro',
  Story = 'Story',
  Ad = 'Ad',
}
export const eveningOverviewItemTypeOptions = [
  new OptionItem('Intro', EveningOverviewItemType.Intro),
  new OptionItem('Story', EveningOverviewItemType.Story),
  new OptionItem('Ad', EveningOverviewItemType.Ad),
];
