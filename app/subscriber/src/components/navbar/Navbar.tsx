import _ from 'lodash';
import React from 'react';
import { FaAnglesLeft, FaAnglesRight } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { Row, Show } from 'tno-core';

import { INavbarOptions, navbarOptions } from './NavbarItems';
import * as styled from './styled';
import { determineGroupIcon } from './utils/determineGroupIcon';

export interface INavbarGroup {
  groupName: string;
  sortOrder: number;
}

export interface INavbarProps {
  options?: INavbarOptions[];
}

export const Navbar: React.FC<INavbarProps> = ({ options }) => {
  const [expanded, setExpanded] = React.useState(true);
  // groupless options will appear first, followed by grouped options
  const grouplessOptions = navbarOptions?.filter((option) => !option.groupName);
  const groupedOptions = navbarOptions?.filter((option) => !!option.groupName);
  const navigate = useNavigate();

  const result = _.groupBy(groupedOptions, 'groupName');
  return (
    <styled.Navbar $expanded={expanded}>
      <>
        {grouplessOptions?.map((option, index) => {
          return (
            <Row className="option" key={index} onClick={() => navigate(option.path)}>
              {option.icon}
              <Show visible={expanded}>{option.label}</Show>
            </Row>
          );
        })}

        {Object.keys(result).map((key) => {
          console.log(key);
          return (
            <span className="group-section">
              <div className="group-title">
                {determineGroupIcon(key)}
                <Show visible={expanded}>{key}</Show>
              </div>
              {result[key].map((option, index) => {
                return (
                  <Row
                    className={`option ${key.replace(/\s+/g, '-').toLowerCase()}`}
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
        {expanded ? (
          <FaAnglesLeft className="expand-control" onClick={() => setExpanded(false)} />
        ) : (
          <FaAnglesRight className="expand-control" onClick={() => setExpanded(true)} />
        )}
      </>
    </styled.Navbar>
  );
};
