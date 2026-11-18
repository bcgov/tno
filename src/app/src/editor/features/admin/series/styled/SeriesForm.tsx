import styled from 'styled-components';

import { FormPage } from '@/editor/components/formpage';

export const SeriesForm = styled(FormPage)`
  display: flex;
  flex-direction: column;
  align-items: center;

  .back-button {
    align-self: start;
    margin-bottom: 0.5rem;
  }
`;
