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
}

export const Tabs: React.FC<ITabsProps> = ({
  tabs,
  activeTab = 0,
  className,
  children,
  ...rest
}) => {
  const navigate = useNavigate();
  const [active, setActive] = React.useState(getTab(tabs, activeTab));
  const { pathname } = useLocation();

  React.useEffect(() => {
    const tab = tabs.find((t) => t.to === pathname);
    if (tab) setActive(tab);
  }, [pathname, tabs]);

  return (
    <styled.Tabs className={`tabs${className ? ` ${className}` : ''}`} activeTab={active}>
      <div className="tabs-header">
        {tabs.map((tab) => (
          <div
            key={tab.key}
            className={`tab ${tab.className}${active?.key === tab.key ? ' active' : ''}`}
          >
            <div
              onClick={() => {
                if (tab.type !== 'other') setActive(tab);
                if (tab.onClick) {
                  tab.onClick(tab);
                } else if (tab.to) {
                  navigate(tab.to);
                }
              }}
            >
              {tab.label}
            </div>
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
