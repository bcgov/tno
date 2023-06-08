import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowDown, FaArrowUp, FaTrash } from 'react-icons/fa';
import { Button, ButtonVariant, Col, IReportModel, Row } from 'tno-core';

import { IReportSection } from './interfaces';
import { ReportSection } from './ReportSection';
import * as styled from './styled';

const defaultSection = (sortOrder: number): IReportSection => {
  return {
    name: '',
    label: '',
    description: '',
    sortOrder: sortOrder,
    isEnabled: true,
    filter: {},
  };
};

export const ReportSections = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  const sections: IReportSection[] = values.settings.sections ?? [];
  const [section, setSection] = React.useState<IReportSection>();

  return (
    <styled.ReportSections>
      <p>
        A report can contain multiple sections, each with its own content and filter. Each section
        name must be unique. The template will use this name to reference the section.
      </p>
      <Col alignItems="flex-end">
        <Button
          variant={ButtonVariant.secondary}
          onClick={() => {
            const values = [...sections, defaultSection(sections.length)];
            setFieldValue('settings.sections', values);
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
          {sections.map((row, index) => (
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
                      disabled={index < 1 || sections.length < index}
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        var results = [...sections];
                        var above = results[index - 1];
                        results.splice(index, 1);
                        results.splice(index - 1, 0, {
                          ...row,
                          sortOrder: above.sortOrder,
                        });
                        setFieldValue(
                          'settings.sections',
                          results.map((r, i) => ({ ...r, sortOrder: i })),
                        );
                      }}
                    >
                      <FaArrowUp />
                    </Button>
                    <Button
                      variant={ButtonVariant.link}
                      className="move"
                      disabled={index >= sections.length - 1}
                      onClick={async (e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        var results = [...sections];
                        var below = results[index + 1];
                        results.splice(index, 1);
                        results.splice(index + 1, 0, {
                          ...row,
                          sortOrder: below.sortOrder,
                        });
                        below.sortOrder--;
                        setFieldValue(
                          'settings.sections',
                          results.map((r, i) => ({ ...r, sortOrder: i })),
                        );
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
                      sections.splice(index, 1);
                      if (index === section?.sortOrder) setSection(undefined);
                      const values = sections.map((s, i) => ({ ...s, sortOrder: i }));
                      setFieldValue(`settings.sections`, values);
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
