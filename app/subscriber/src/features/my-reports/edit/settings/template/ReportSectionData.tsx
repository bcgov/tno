import React from 'react';
import { FaChevronDown, FaChevronUp } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  IOptionItem,
  OptionItem,
  ReportSectionDataTypeOptions,
  Row,
  Show,
} from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';
import {
  TalkwalkerCollageTemplate,
  TalkwalkerPostListTemplate,
  TalkwalkerSummaryTemplate,
} from './constants';

export interface IReportSectionDataProps {
  index: number;
}

export const ReportSectionData = React.forwardRef<HTMLDivElement, IReportSectionDataProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useReportEditContext();

    const [dataTypeOptions] = React.useState<IOptionItem[]>(ReportSectionDataTypeOptions);
    const [showAdvanced, setShowAdvanced] = React.useState(false);

    React.useEffect(() => {
      setFieldValue(`sections.${index}.settings.preload`, true);
    }, [setFieldValue, index]);

    return (
      <>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <Row>
          <Col className="frm-in" flex="1" gap="1rem">
            <Col>
              <label>Report Section Options</label>
              <FormikCheckbox name={`sections.${index}.isEnabled`} label="Section is visible" />
              <Checkbox
                name={`sections.${index}.settings.inTableOfContents`}
                label="Include in Table of Contents"
                checked={
                  values.sections[index].settings.inTableOfContents === undefined
                    ? true
                    : values.sections[index].settings.inTableOfContents
                }
                onChange={(e) => {
                  setFieldValue(`sections.${index}.settings.inTableOfContents`, e.target.checked);
                }}
              />
            </Col>
            <Row>
              <Col flex="1">
                <FormikText required label="Url" name={`sections.${index}.settings.url`} />
              </Col>
              <FormikSelect
                name={`sections.${index}.settings.dataType`}
                label="Data Type"
                required
                options={dataTypeOptions}
                value={
                  dataTypeOptions.find(
                    (o) => o.value === values.sections[index].settings.dataType,
                  ) ?? ''
                }
                onChange={(newValue) => {
                  const option = newValue as OptionItem;
                  const dataType = dataTypeOptions.find((f) => f.value === option?.value);
                  if (dataType)
                    setFieldValue(`sections.${index}.settings.dataType`, dataType.value);
                  if (!['xml', 'json'].includes(`${dataType?.value}`))
                    setFieldValue(`sections.${index}.settings.dataProperty`, '');
                }}
              />
              <Col className="row-btn"></Col>
            </Row>
            <Col gap="1rem" className="data-template-editor">
              <Row gap="1rem;" justifyContent="space-between">
                <Row gap="1rem">
                  <p>Default Templates:</p>
                  <Show visible={values.sections[index].settings.dataType === 'json'}>
                    <Button
                      variant={ButtonVariant.secondary}
                      onClick={() => {
                        setFieldValue(
                          `sections.${index}.settings.dataTemplate`,
                          TalkwalkerSummaryTemplate,
                        );
                      }}
                    >
                      Talkwalker Summary
                    </Button>
                  </Show>
                  <Show visible={values.sections[index].settings.dataType === 'csv'}>
                    <Button
                      variant={ButtonVariant.secondary}
                      onClick={() => {
                        setFieldValue(
                          `sections.${index}.settings.dataTemplate`,
                          TalkwalkerCollageTemplate,
                        );
                      }}
                    >
                      Talkwalker Collage
                    </Button>
                    <Button
                      variant={ButtonVariant.secondary}
                      onClick={() => {
                        setFieldValue(
                          `sections.${index}.settings.dataTemplate`,
                          TalkwalkerPostListTemplate,
                        );
                      }}
                    >
                      Talkwalker Post List
                    </Button>
                  </Show>
                </Row>
                <Row gap="1rem">
                  <p>Show advanced editor</p>
                  <Button
                    variant={ButtonVariant.secondary}
                    title="Show Advanced Editor"
                    onClick={() => setShowAdvanced((state) => !state)}
                  >
                    {showAdvanced ? <FaChevronUp /> : <FaChevronDown />}
                  </Button>
                </Row>
              </Row>
              <Show visible={showAdvanced}>
                <Show
                  visible={['xml', 'json'].includes(values.sections[index].settings.dataType ?? '')}
                >
                  <FormikText
                    label="Path to Property"
                    name={`sections.${index}.settings.dataProperty`}
                    tooltip="XPath to the property in the data you want displayed."
                    placeholder="Optional XPath to a single value.  If a path is provided it will be used instead of the template."
                  />
                </Show>
                <p>
                  The Razor template is a C# syntax that enables the ability to convert the source
                  data into HTML.
                </p>
                <FormikTextArea
                  label="Razor Template"
                  name={`sections.${index}.settings.dataTemplate`}
                />
              </Show>
            </Col>
          </Col>
        </Row>
      </>
    );
  },
);
