import { BrowserRouter } from 'react-router-dom';
import css from './css/_variables.module.scss';
import { ThemeProvider } from 'styled-components';

export interface ITestWrapperProps {
  children?: React.ReactNode;
}

export const TestWrapper: React.FC<ITestWrapperProps> = ({ children }) => {
  return (
    <BrowserRouter>
      <ThemeProvider theme={{ css }}>{children}</ThemeProvider>
    </BrowserRouter>
  );
};
