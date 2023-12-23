export interface ITab {
  key: string;
  label?: React.ReactNode;
  icon?: React.ReactNode;
  to?: string;
  type?: 'tab' | 'other';
  className?: string;
  onClick?: (tab: ITab, event: React.MouseEvent<HTMLDivElement, MouseEvent>) => void;
}
