import styled from 'styled-components';

import { FormPage } from '@/editor/components/formpage';

export const ContributorForm = styled(FormPage)`
  .is-press {
    margin-left: 1rem;
  }
  display: flex;
  flex-direction: column;
  align-items: center;

  .back-button {
    align-self: start;
  }
`;
