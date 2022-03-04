import { TextBox } from 'components/text-box';
import { LoginPanel } from 'features/login';

import { InfoPanel, InfoText, LogoPanel } from '.';

export const Home = () => {
  return (
    <TextBox className="home" height="618px" width="1200px" backgroundColor="#FFFFFF">
      <LogoPanel width="33%" backgroundColor="#003366" />
      <InfoPanel width="33%" backgroundColor="#F5F5F5">
        <InfoText />
      </InfoPanel>
      <InfoPanel width="33%" backgroundColor="#FFFFFF" roundedEdges>
        <LoginPanel />
      </InfoPanel>
    </TextBox>
  );
};
