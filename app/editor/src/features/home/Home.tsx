import { TextBox } from 'components/text-box';
import { LoginPanel } from 'features/login';

import { InfoPanel, InfoText, LogoPanel } from '.';

export const Home = () => {
  return (
    <TextBox className="home" height="618px" width="1200px" backgroundColor="#FFFFFF">
      <LogoPanel backgroundColor="#003366" />
      <InfoPanel backgroundColor="#F5F5F5">
        <InfoText />
      </InfoPanel>
      <InfoPanel backgroundColor="#FFFFFF" roundedEdges>
        <LoginPanel />
      </InfoPanel>
    </TextBox>
  );
};
