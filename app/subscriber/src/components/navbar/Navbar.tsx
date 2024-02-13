import { InfoShield } from 'components/info';
import _ from 'lodash';
import React from 'react';
import { FaAnglesLeft, FaAnglesRight } from 'react-icons/fa6';
import { useLocation, useNavigate } from 'react-router-dom';
import { Row, Show, useWindowSize } from 'tno-core';

import { INavbarItem } from './constants/INavbarItem';
import * as styled from './styled';
import { determineGroupIcon } from './utils/determineGroupIcon';

export interface INavbarGroup {
  groupName: string;
  sortOrder: number;
}

export interface INavbarProps {
  options: INavbarItem[];
}

/**
 * Navbar component to navigate through the application
 * @param options - Array of INavbarItem objects
 * @returns - Navbar component
 */
export const Navbar: React.FC<INavbarProps> = ({ options }) => {
  const navigate = useNavigate();

  const [expanded, setExpanded] = React.useState(true);
  const { pathname } = useLocation();
  const { width } = useWindowSize();

  const grouplessOptions = options?.filter((option) => !option.groupName);
  const groupedOptions = options?.filter((option) => !!option.groupName);
  const groupByName = _.groupBy(groupedOptions, 'groupName');

  const determineClassName = React.useCallback(
    (path: string) => {
      if (pathname.includes(path)) return 'active';
      return '';
    },
    [pathname],
  );

  React.useEffect(() => {
    if (width && width < 700) setExpanded(false);
  }, [width]);

  return (
    <styled.Navbar $expanded={expanded}>
      <>
        {grouplessOptions?.map((option, index) => {
          return (
            <Row
              className={`option ${determineClassName(option.path)}`}
              key={index}
              onClick={() => navigate(option.path)}
            >
              {option.icon}
              <Show visible={expanded}>{option.label}</Show>
            </Row>
          );
        })}

        {Object.keys(groupByName).map((key) => {
          return (
            <span key={key} className="group-section">
              <div className="group-title">
                {determineGroupIcon(key)}
                <Show visible={expanded}>{key}</Show>
              </div>
              {groupByName[key].map((option, index) => {
                return (
                  <Row
                    className={`option ${key
                      .replace(/\s+/g, '-')
                      .toLowerCase()} ${determineClassName(option.path)}`}
                    key={index}
                    onClick={() => navigate(option.path)}
                  >
                    {option.icon}
                    <Show visible={expanded}>{option.label}</Show>
                    <Show visible={!!option.secondaryIcon && expanded}>
                      <span
                        onClick={(e) => {
                          e.stopPropagation();
                          !!option.secondaryIconRoute && navigate(option.secondaryIconRoute);
                        }}
                        className="secondary-icon"
                      >
                        {option.secondaryIcon}
                      </span>
                    </Show>
                  </Row>
                );
              })}
            </span>
          );
        })}
        <Show visible={!!width && width > 700}>
          {expanded ? (
            <FaAnglesLeft className="expand-control" onClick={() => setExpanded(false)} />
          ) : (
            <FaAnglesRight className="expand-control" onClick={() => setExpanded(true)} />
          )}
        </Show>
        <InfoShield />
      </>
    </styled.Navbar>
  );
};
