import styled from 'styled-components';

import { ICheckboxProps } from '..';
import { LabelPosition } from '../constants';

export const SmallCheckbox = styled.div<ICheckboxProps>`
  & > div:first-child {
    display: flex;
    align-items: center;
    flex-direction: ${(props) => {
      switch (props.labelPosition) {
        case LabelPosition.Top:
        case LabelPosition.Bottom:
          return 'column';
        case LabelPosition.Left:
        case LabelPosition.Right:
        default:
          return 'row';
      }
    }};
  }

  label {
    ${(props) => {
      switch (props.labelPosition) {
        case LabelPosition.Right:
          return 'padding-left: 0.25em;';
        case LabelPosition.Left:
          return 'padding-right: 0.25em;';
        default:
          return '';
      }
    }}
    font-weight: normal;
  }

  input[role='alert'] {
    border-color: ${(props) => props.theme.css.dangerColor};
    filter: grayscale(100%) brightness(65%) sepia(25%) hue-rotate(-50deg) saturate(600%)
      contrast(0.8);
  }

  &:hover {
    color: ${(props) => props.theme.css.primaryColor};
  }
`;
