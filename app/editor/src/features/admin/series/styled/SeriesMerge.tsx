import { FormPage } from 'components/formpage';
import styled from 'styled-components';

export const SeriesMerge = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  .warning {
    border: 2px solid ${(props) => props.theme.css.dangerColor};
    font-weight: bold;
    padding: 0.5rem;
    border-radius: 0.25rem;
  }
`;
