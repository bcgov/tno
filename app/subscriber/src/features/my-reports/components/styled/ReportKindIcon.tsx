import styled from 'styled-components';

export const ReportKindIcon = styled.div`
  color: ${(props) => props.theme.css.iconPrimaryColor};

  &.active {
    color: ${(props) => props.theme.css.iconGreen};
  }
`;
