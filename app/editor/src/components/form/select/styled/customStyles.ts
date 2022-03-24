import { StylesConfig } from 'react-select';

export const customStyles: StylesConfig = {
  control: (provided, state) => ({
    ...provided,
    background: '#f2f2f2',
    borderColor: '#38598a',
    width: 200,
    boxShadow: state.isFocused ? '0 0 0 0.2rem rgb(86 114 156 / 50%)' : 'none',

    '&:hover': {
      borderColor: '#38598a',
    },
  }),
  menu: (provided, state) => ({
    ...provided,
    width: 200,
  }),
  option: (provided, state) => ({
    ...provided,
    background: state.isSelected ? '#848884' : state.isFocused ? '#ddd' : 'transparent',

    '&:hover': {
      background: state.isSelected ? '#848884' : '#ddd',
    },
  }),
};
