import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowDown, FaArrowUp, FaTrash } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  IReportModel,
  IReportSectionModel,
  Row,
} from 'tno-core';

import { defaultReportSection } from './constants';
import { ReportSection } from './ReportSection';
import * as styled from './styled';

export const ReportSections = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  const [section, setSection] = React.useState<IReportSectionModel>();

  const handleAddSection = () => {
    setFieldValue(
      'sections',
      [...values.sections, defaultReportSection(values.id)].map((section, index) => {
        return {
          ...section,
          name: `section-${index + 1}`,
          sortOrder: index,
        };
      }),
    );
  };

  const handleMoveUp = (section: IReportSectionModel, index: number) => {
    var results = [...values.sections];
    var above = results[index - 1];
    results.splice(index, 1);
    results.splice(index - 1, 0, {
      ...section,
      sortOrder: above.sortOrder,
    });
    setFieldValue(
      'sections',
      results.map((r, i) => ({ ...r, sortOrder: i })),
    );
  };

  const handleMoveDown = (section: IReportSectionModel, index: number) => {
    var results = [...values.sections];
    var below = results[index + 1];
    results.splice(index, 1);
    results.splice(index + 1, 0, {
      ...section,
      sortOrder: below.sortOrder,
    });
    below.sortOrder--;
    setFieldValue(
      'sections',
      results.map((r, i) => ({ ...r, sortOrder: i })),
    );
  };

  const handleDelete = (index: number) => {
    var sections = [...values.sections];
    sections.splice(index, 1);
    if (index === section?.sortOrder) setSection(undefined);
    setFieldValue(
      `sections`,
      sections.map((s, i) => ({ ...s, name: `section-${i + 1}`, sortOrder: i })),
    );
  };

  return (
    <styled.ReportSections>
      <p>A report must contain one or more section to display content or charts.</p>
      <Col className="frm-in">
        <label>Section Options</label>
        <Row>
          <FormikCheckbox
            label="Hide Empty"
            name="settings.sections.hideEmpty"
            tooltip="If there is no content in the section it will not be included."
          />
          <FormikCheckbox
            label="Use Page Breaks"
            name="settings.sections.usePageBreaks"
            tooltip="use page breaks in each section for printing."
          />
          <Col flex="1" alignItems="flex-end">
            <Button variant={ButtonVariant.secondary} onClick={handleAddSection}>
              Add New Section
            </Button>
          </Col>
        </Row>
      </Col>
      <Col>
        <div className="section-table">
          <Row className="row-header">
            <Col className="st-1"></Col>
            <Col className="st-2">Heading</Col>
            <Col className="st-3">Description</Col>
            <Col className="st-4"></Col>
          </Row>
          {values.sections.map((row, index) => (
            <React.Fragment key={index}>
              <Row
                className={`row${row.sortOrder === section?.sortOrder ? ' active' : ''}`}
                onClick={() => {
                  if (row.sortOrder !== section?.sortOrder) setSection(row);
                  else setSection(undefined);
                }}
              >
                <Col className="st-1">{index + 1}</Col>
                <Col className="st-2">{row.settings.label}</Col>
                <Col className="st-3">{row.description}</Col>
                <Col className="st-4">
                  <Col>
                    <Button
                      variant={ButtonVariant.link}
                      className="move"
                      disabled={index < 1 || values.sections.length < index}
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        handleMoveUp(row, index);
                      }}
                    >
                      <FaArrowUp />
                    </Button>
                    <Button
                      variant={ButtonVariant.link}
                      className="move"
                      disabled={index >= values.sections.length - 1}
                      onClick={async (e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        handleMoveDown(row, index);
                      }}
                    >
                      <FaArrowDown />
                    </Button>
                  </Col>
                  <Button
                    variant={ButtonVariant.danger}
                    onClick={async (e) => {
                      e.preventDefault();
                      e.stopPropagation();
                      handleDelete(index);
                    }}
                  >
                    <FaTrash />
                  </Button>
                </Col>
              </Row>
              {!!section && section.sortOrder === row.sortOrder && (
                <ReportSection className="section" index={index} />
              )}
            </React.Fragment>
          ))}
        </div>
      </Col>
    </styled.ReportSections>
  );
};
