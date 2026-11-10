import { ContentActionBar, IContentActionBarProps } from './ContentActionBar';

export const ContentListActionBar: React.FC<IContentActionBarProps> = ({ className, ...rest }) => {
  return <ContentActionBar className={`list-view ${className}`} {...rest} />;
};
