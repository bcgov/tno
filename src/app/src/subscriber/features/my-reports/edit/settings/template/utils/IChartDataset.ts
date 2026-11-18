export interface IChartDataset {
  label: string;
  data: (number | undefined | null)[];
  backgroundColor?: string[];
  borderColor?: string[];
  spanGaps?: boolean;
  borderWidth?: number;
  minBarLength?: string;
}
