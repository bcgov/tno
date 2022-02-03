import { InfoPanel, InfoText, LogoPanel, TextBox } from 'components';
import { LoginPanel } from 'components/home/LoginPanel';

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
