import React from 'react';
import { Col, Row } from '../../flex';
import { Show } from '../../show';
import { FaCode, FaExpandAlt, FaPaintBrush, FaRemoveFormat } from 'react-icons/fa';
import { IUrlOption } from './interfaces';
import { FaCircleXmark, FaRegNewspaper } from 'react-icons/fa6';
export interface ICustomToolbarProps {
  onClickRaw: React.MouseEventHandler<HTMLButtonElement>;
  onClickRemoveFormat: React.MouseEventHandler<HTMLButtonElement>;
  onClickFormatRaw: React.MouseEventHandler<HTMLButtonElement>;
  onChangeContentSelect: (value: IUrlOption) => void;
  /** options to choose from a dropdown that will hyperlink the url attribute */
  urlOptions?: IUrlOption[];
  onClickClear: React.MouseEventHandler<HTMLButtonElement>;
  onClickExpand: React.MouseEventHandler<HTMLButtonElement>;
  innerRef: any;
}

/** Custom toolbar for the content WYSIWYG including ability to view HTML source and clear formatting */
export const CustomToolbar: React.FC<ICustomToolbarProps> = ({
  onClickRaw,
  onClickRemoveFormat,
  onClickFormatRaw,
  urlOptions,
  onClickExpand,
  onChangeContentSelect,
  innerRef,
}) => {
  const groupedOptions =
    urlOptions?.reduce((acc, option) => {
      if (!acc[option.section]) {
        acc[option.section] = [];
      }
      acc[option.section].push(option);
      return acc;
    }, {} as { [key: string]: IUrlOption[] }) || {}; // Ensures groupedOptions is an empty object if urlOptions is undefined
  const [showContentSelect, setShowContentSelect] = React.useState(false);

  return (
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
      <span className="ql-formats">
        <Show visible={!!urlOptions?.length}>
          <Col>
            <div
              className={`add-button ${showContentSelect && 'hide-bottom-border'}`}
              onClick={() => setShowContentSelect(true)}
            >
              <FaRegNewspaper /> <span className="add-text">Insert Link to Story </span>
            </div>
            <Show visible={showContentSelect}>
              <div className="content-menu">
                <Row>
                  <FaCircleXmark onClick={() => setShowContentSelect(false)} className="exit" />
                </Row>
                {Object.keys(groupedOptions).map((category) => (
                  <Col key={category}>
                    <b>{category}</b>
                    {groupedOptions[category].map((option) => (
                      <div className="content-option" onClick={() => onChangeContentSelect(option)}>
                        {option.label}
                      </div>
                    ))}
                  </Col>
                ))}
              </div>
            </Show>
          </Col>
        </Show>
      </span>
    </div>
  );
};
