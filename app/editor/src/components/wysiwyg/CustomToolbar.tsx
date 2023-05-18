import { FaCode, FaExpandAlt, FaPaintBrush, FaRemoveFormat } from 'react-icons/fa';
export interface ICustomToolbarProps {
  onClickRaw: React.MouseEventHandler<HTMLButtonElement>;
  onClickRemoveFormat: React.MouseEventHandler<HTMLButtonElement>;
  onClickFormatRaw: React.MouseEventHandler<HTMLButtonElement>;
  onClickClear: React.MouseEventHandler<HTMLButtonElement>;
  onClickExpand: React.MouseEventHandler<HTMLButtonElement>;
  innerRef: any;
}

/** Custom toolbar for the content WYSIWYG including ability to view HTML source and clear formatting */
export const CustomToolbar: React.FC<ICustomToolbarProps> = ({
  onClickRaw,
  onClickRemoveFormat,
  onClickFormatRaw,
  onClickClear,
  onClickExpand,
  innerRef,
}) => (
  <div ref={innerRef} className="toolbar">
    <span className="ql-formats">
      <select className="ql-header" title="Format Type" />
      <button className="ql-bold" title="Bold Text" />
      <button className="ql-italic" title="Italic Text" />
      <button className="ql-underline" title="Underline Text" />
      <button className="ql-strike" title="Strikethrough Text" />
    </span>
    <span className="ql-formats">
      <select className="ql-align" title="Alignment" />
      <select className="ql-color" title="Text colour" />
    </span>
    <span className="ql-formats">
      <button className="ql-list" value="ordered" title="Numbered List" />
      <button className="ql-list" value="bullet" title="Bulleted list" />
      <button className="ql-indent" value="-1" title="Outdent" />
      <button className="ql-indent" value="+1" title="Indent" />
    </span>
    <span className="ql-formats">
      <button type="button" onClick={onClickRaw} title="Show code">
        <FaCode className="custom-icon" />
      </button>
      <button type="button" onClick={onClickRemoveFormat} title="Remove formatting">
        <FaRemoveFormat className="custom-icon" />
      </button>
      <button type="button" onClick={onClickFormatRaw} title="Format raw html">
        <FaPaintBrush className="custom-icon" />
      </button>
      <button className="ql-link" title="Link"></button>
      <button className="ql-image" title="Insert image"></button>
    </span>
    <span className="ql-formats">
      <button type="button" onClick={onClickExpand} title="Popout editor">
        <FaExpandAlt className="custom-icon" />
      </button>
    </span>
  </div>
);
