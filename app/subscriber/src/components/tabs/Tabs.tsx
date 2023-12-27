import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

import { ITab } from './interfaces';
import * as styled from './styled';
import { getTab } from './utils';

export interface ITabsProps {
  tabs: ITab[];
  activeTab?: ITab | string | number;
  children?: React.ReactNode | ((props?: ITab) => React.ReactNode);
  className?: string;
  onChange?: (tab: ITab, active?: ITab) => Promise<boolean>;
}

export const Tabs: React.FC<ITabsProps> = ({
  tabs,
  activeTab = 0,
  className,
  children,
  onChange = () => Promise<boolean>,
  ...rest
}) => {
  const navigate = useNavigate();
  const [active, setActive] = React.useState(getTab(tabs, activeTab));
  const { pathname } = useLocation();

  React.useEffect(() => {
    const tab = tabs.find((t) => t.to === pathname);
    if (tab) setActive(tab);
  }, [pathname, tabs]);

  const handleClick = React.useCallback(
    async (tab: ITab, event: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
      if (active?.key === tab.key) return;
      const allow = await onChange(tab, active);
      if (!allow) return;
      if (tab.type !== 'other') setActive(tab);
      if (tab.onClick) {
        tab.onClick(tab, event);
      } else if (tab.to) {
        navigate(tab.to);
      }
    },
    [active, navigate, onChange],
  );

  return (
    <styled.Tabs className={`tabs${className ? ` ${className}` : ''}`} activeTab={active}>
      <div className="tabs-header">
        {tabs.map((tab) => (
          <div
            key={tab.key}
            className={`tab ${tab.className}${active?.key === tab.key ? ' active' : ''}`}
          >
            <div onClick={(e) => handleClick(tab, e)}>{tab.label}</div>
          </div>
        ))}
      </div>
      {typeof children === 'function' ? (
        <div className="tab-container">
          {(children as (props?: ITab) => React.ReactNode)(active)}
        </div>
      ) : (
        <div className="tab-container">{children}</div> ?? <></>
      )}
    </styled.Tabs>
  );
};
