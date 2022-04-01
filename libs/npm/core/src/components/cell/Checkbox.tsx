import React from 'react';
import { FaCheckSquare, FaRegSquare } from 'react-icons/fa';

interface ICheckboxProps {
  checked: boolean;
}

export const Checkbox: React.FC<ICheckboxProps> = ({ checked }) => {
  return checked ? <FaCheckSquare /> : <FaRegSquare />;
};
