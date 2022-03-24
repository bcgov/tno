import { Col } from 'components/flex/col';
import styled from 'styled-components';

export const Select = styled(Col)`
  background-color: ${(props) => props.theme.css.formBackgroundColor};

  p[role='alert'] {
    font-size: 0.85em;
    color: ${(props) => props.theme.css.dangerColor};
  }
`;
