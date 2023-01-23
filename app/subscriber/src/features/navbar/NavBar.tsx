import React from 'react';
import { NavBarGroup } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex/row';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  const [, setActiveHover] = React.useState<string>('');

  const hideRef = React.useRef(false);
  const ref = React.useRef<any>();

  React.useEffect(() => {
    document.addEventListener('click', handleClickOutside, false);
    return () => {
      document.removeEventListener('click', handleClickOutside, false);
    };
  });

  const onMouseOver = () => {
    hideRef.current = false;
  };

  const onMouseLeave = () => {
    hideRef.current = true;
    setTimeout(() => {
      if (hideRef.current) setActiveHover('');
    }, 2000);
  };

  const handleClickOutside = (event: { target: any }) => {
    if (ref.current && !ref.current.contains(event.target)) {
      hideRef.current = true;
      setActiveHover('');
    }
  };

  // const handleClick = (menu: string = '') => {
  //   if (activeHover === menu) setActiveHover('');
  //   else setActiveHover(menu);
  // };

  return (
    <div onMouseLeave={onMouseLeave} onMouseOver={onMouseOver} ref={ref}>
      <NavBarGroup className="navbar">
        <Row></Row>
      </NavBarGroup>
    </div>
  );
};
