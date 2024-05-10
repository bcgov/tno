import { Action } from 'components/action';
import React from 'react';
import { SketchPicker } from 'react-color';
import { FaPalette } from 'react-icons/fa6';
import CreatableSelect from 'react-select/creatable';
import { Col, Row } from 'tno-core';

import * as styled from './styled';
import { setArrayValue } from './utils';

export interface IColoursProps {
  name?: string;
  label?: string;
  options: string[];
  values?: string[];
  className?: string;
  placeholders?: string[];
  onChange?: (newValue: string | undefined | null, values: string[]) => void;
  onOpenPicker?: () => void;
  onClosePicker?: () => void;
}

export const initializeValues = (values: string[]) => {
  return Array.apply(null, Array((values?.length ?? 0) + 1)).map<string | undefined | null>(
    (_, i) => (values && values.length > i ? values[i] : undefined),
  );
};

export const Colours = ({
  name,
  label,
  options: initOptions,
  values: initValues,
  className,
  placeholders,
  onChange,
  onOpenPicker,
  onClosePicker,
}: IColoursProps) => {
  const [options, setOptions] = React.useState<string[]>([]);
  const [values, setValues] = React.useState<(string | undefined | null)[]>(
    initializeValues(initValues ?? []),
  );
  const [showPicker, setShowPicker] = React.useState<number>();

  React.useEffect(() => {
    setOptions(initOptions ?? []);
  }, [initOptions]);

  React.useEffect(() => {
    setValues(initializeValues(initValues ?? []));
  }, [initValues]);

  const handleChange = React.useCallback(
    (newValue: string | undefined | null, index: number) => {
      let newArray = setArrayValue(values ?? [], newValue, index);
      if (!newArray.length) newArray = [undefined];
      else if (![undefined, null, ''].includes(newArray[newArray.length - 1]))
        newArray = [...newArray, undefined];

      const result: string[] = newArray
        .filter((v) => ![undefined, null, ''].includes(v))
        .map((v) => v!);

      if (newValue && !options.some((o) => o === newValue)) {
        setOptions((options) => [...options, newValue]);
      }

      if (onChange) onChange(newValue, result);
      else setValues(newArray);
    },
    [onChange, options, values],
  );

  const optionItems = options.map((o) => ({
    label: o,
    value: o,
  }));

  return (
    <styled.Colours className={`frm-in${className ? ` ${className}` : ''}`}>
      {label && <label>{label}</label>}
      {values.map((itemValue, index) => {
        return (
          <Row key={`${itemValue}-${index}`}>
            <Row flex="1" gap="0.25rem">
              <Col flex="1">
                <CreatableSelect
                  name={`${name}-${index}`}
                  options={optionItems}
                  isClearable
                  placeholder={
                    placeholders && placeholders.length > index ? placeholders[index] : undefined
                  }
                  value={itemValue ? { label: itemValue, value: itemValue } : undefined}
                  className="select"
                  classNamePrefix="select"
                  onChange={(newValue) => handleChange(newValue?.value, index)}
                />
              </Col>
              <Action
                icon={<FaPalette />}
                onClick={() => {
                  setShowPicker(index);
                  onOpenPicker?.();
                }}
              />
              {showPicker === index && (
                <div
                  className="picker-overlay"
                  onClick={(e) => {
                    setShowPicker(undefined);
                    onClosePicker?.();
                  }}
                >
                  <div
                    onClick={(e) => {
                      e.preventDefault();
                      e.stopPropagation();
                    }}
                  >
                    <SketchPicker
                      className="picker"
                      onChangeComplete={(color) => {
                        handleChange(color.hex, index);
                        // setShowPicker(undefined);
                        // onClosePicker?.();
                      }}
                      color={itemValue ?? undefined}
                    />
                  </div>
                </div>
              )}
            </Row>
          </Row>
        );
      })}
    </styled.Colours>
  );
};
