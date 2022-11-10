import { FaCode, FaRemoveFormat } from 'react-icons/fa';

export interface ICustomToolbarProps {
  onClickRaw: React.MouseEventHandler<HTMLButtonElement>;
  onClickRemoveFormat: React.MouseEventHandler<HTMLButtonElement>;
}

/** Custom toolbar for the content WYSIWYG including ability to view HTML source and clear formatting */
export const CustomToolbar: React.FC<ICustomToolbarProps> = ({
  onClickRaw,
  onClickRemoveFormat,
}) => (
  <div id="toolbar" className="toolbar">
    <select className="ql-header" />
    <button className="ql-bold"></button>
    <button className="ql-italic"></button>
    <select className="ql-color" />
    <button className="ql-link"></button>
    <button className="ql-underline"></button>
    <button type="button" onClick={onClickRaw}>
      <FaCode className="custom-icon" />
    </button>
    <button type="button" onClick={onClickRemoveFormat}>
      <FaRemoveFormat className="custom-icon" />
    </button>
  </div>
);
