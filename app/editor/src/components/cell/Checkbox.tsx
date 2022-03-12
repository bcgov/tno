import React from 'react';
import { FaCheckSquare, FaRegSquare } from 'react-icons/fa';

interface ICheckboxProps {
  value: boolean;
}

export const Checkbox: React.FC<ICheckboxProps> = ({ value }) => {
  return value ? <FaCheckSquare /> : <FaRegSquare />;
};
