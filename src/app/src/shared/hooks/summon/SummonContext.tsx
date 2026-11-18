import React from 'react';

interface ISummonProviderProps extends React.HTMLAttributes<HTMLElement> {
  token?: string | null;
  authReady?: boolean;
}

interface ISummonState {
  authReady: boolean;
  setAuthReady: React.Dispatch<React.SetStateAction<boolean>>;
  token?: string | null;
  setToken: React.Dispatch<React.SetStateAction<string | null | undefined>>;
}

/**
 * SummonContext, provides shared state between AJAX requests.
 */
export const SummonContext = React.createContext<ISummonState>({
  authReady: false,
  setAuthReady: () => {},
  setToken: () => {},
});

/**
 * SummonProvider, provides a way to initialize context.
 * @param param0 SummonProvider initialization properties.
 * @returns
 */
export const SummonProvider: React.FC<ISummonProviderProps> = ({
  authReady: initAuthReady = false,
  token: initToken,
  children,
}) => {
  const [token, setToken] = React.useState<string | null | undefined>(initToken);
  const [authReady, setAuthReady] = React.useState<boolean>(initAuthReady);
  return (
    <SummonContext.Provider value={{ authReady, setAuthReady, token, setToken }}>
      {children}
    </SummonContext.Provider>
  );
};

export const SummonConsumer = SummonContext.Consumer;
