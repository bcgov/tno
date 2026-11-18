export interface ITab {
  /** Unique key */
  key: string;
  /** Label of tab. */
  label?: React.ReactNode;
  /** Icon to display. */
  icon?: React.ReactNode;
  /** Type of tab */
  type?: 'tab' | 'other';
  /** Class name for styling */
  className?: string;
  /** Navigate to when clicked. */
  to?: string;
  /** Event fires when user clicks tab.  This overrides default onClick behaviour. */
  onClick?: (tab: ITab, event: React.MouseEvent<HTMLDivElement, MouseEvent>) => void;
}
