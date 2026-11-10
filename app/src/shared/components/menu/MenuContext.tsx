import React from 'react';

import { MenuStatus } from './MenuStatus';

interface IMenuProviderProps extends React.HTMLAttributes<HTMLElement> {
  status?: MenuStatus;
}

interface IMenuState {
  status: MenuStatus;
  setStatus: React.Dispatch<React.SetStateAction<MenuStatus>>;
}

/**
 * MenuContext, provides shared state to manage the menu.
 */
export const MenuContext = React.createContext<IMenuState>({
  status: MenuStatus.hidden,
  setStatus: () => {},
});

/**
 * MenuProvider, provides a way to initialize context.
 * @param param0 MenuProvider initialization properties.
 * @returns
 */
export const MenuProvider: React.FC<IMenuProviderProps> = ({
  status: initStatus = MenuStatus.hidden,
  children,
}) => {
  const [status, setStatus] = React.useState<MenuStatus>(initStatus);
  return <MenuContext.Provider value={{ status, setStatus }}>{children}</MenuContext.Provider>;
};

export const MenuConsumer = MenuContext.Consumer;
