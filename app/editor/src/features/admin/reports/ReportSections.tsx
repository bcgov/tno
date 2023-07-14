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
    const results = sections.map((s, i) => ({ ...s, sortOrder: i }));
    setFieldValue(`sections`, results);
  };

  return (
    <styled.ReportSections>
      <p>
        A report must contain one or more section, each with its own content and filter. Each
        section name must be unique. The template will use this name to reference the section.
      </p>
      <Col className="frm-in">
        <label>Section Options</label>
        <Row>
          <FormikCheckbox label="Hide Empty" name="settings.sections.hideEmpty" />
          <FormikCheckbox label="Use Page Breaks" name="settings.sections.usePageBreaks" />
        </Row>
      </Col>
      <Col alignItems="flex-end">
        <Button
          variant={ButtonVariant.secondary}
          onClick={() => {
            setFieldValue(
              'sections',
              [
                ...values.sections,
                defaultReportSection(`section-${values.sections.length + 1}`, values.id),
              ].map((section, index) => {
                return { ...section, sortOrder: index };
              }),
            );
          }}
        >
          Add New Section
        </Button>
      </Col>
      <Col>
        <div className="section-table">
          <Row className="row-header">
            <Col className="st-1">Name</Col>
            <Col className="st-2">Description</Col>
            <Col className="st-3"></Col>
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
                <Col className="st-1">{row.name}</Col>
                <Col className="st-2">{row.description}</Col>
                <Col className="st-3">
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
