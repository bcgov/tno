import { ChartPicker, ConfigureDataset } from '../charts';

export const wizardMenuOptions = [
  {
    id: 'chartType',
    label: 'Choose Chart',
    component: ChartPicker,
  },
  {
    id: 'dataset',
    label: 'Data Configuration',
    component: ConfigureDataset,
  },
  {
    id: 'legend',
    label: 'Dataset Legend',
  },
  {
    id: 'scale',
    label: 'Scale Settings',
  },
  {
    id: 'labels',
    label: 'Chart Labels',
  },
  {
    id: 'colours',
    label: 'Chart Colours',
  },
];
