import React from 'react';
import { FaCheckSquare, FaRegSquare } from 'react-icons/fa';

interface ICellCheckboxProps {
  checked: boolean;
}

export const CellCheckbox: React.FC<ICellCheckboxProps> = ({ checked }) => {
  return checked ? <FaCheckSquare /> : <FaRegSquare />;
};
