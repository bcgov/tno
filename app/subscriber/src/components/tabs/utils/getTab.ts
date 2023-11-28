import { ITab } from '../interfaces';

export const getTab = (tabs: ITab[], tab?: ITab | string | number) => {
  if (!tabs.length) return undefined;

  if (tab === undefined) return tabs.find((t) => !t.type || t.type === 'tab');
  if (typeof tab === 'number') return tabs.length > tab ? tabs[tab] : tabs[0];
  if (typeof tab === 'string') return tabs.find((t) => t.key === tab) ?? tabs[0];
  return tabs.find((t) => t.key === tab.key) ?? tabs[0];
};
