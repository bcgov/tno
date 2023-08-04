import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowDown, FaArrowUp, FaTrash } from 'react-icons/fa';
import { useReports } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  getSortableOptions,
  IReportModel,
  IReportSectionModel,
  OptionItem,
  Row,
} from 'tno-core';

import { defaultReportSection } from './constants';
import { ReportSection } from './ReportSection';
import * as styled from './styled';

export const ReportFormSections = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ reports }, { findAllReports }] = useReports();

  const [section, setSection] = React.useState<IReportSectionModel>();
  const [reportOptions, setReportOptions] = React.useState(getSortableOptions(reports));

  React.useEffect(() => {
    if (!reports.length)
      findAllReports()
        .then((reports) => {
          setReportOptions(getSortableOptions(reports));
        })
        .catch();
    // Only run on initialize.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

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
      <h2>{values.name}</h2>
      <p>A report must contain one or more section to display content or charts.</p>
      <Col className="frm-in">
        <label>Subject Line Options</label>
        <Row alignItems="center">
          <FormikText label="Text" name="settings.subject.text" required />
          <FormikCheckbox
            label="Show Today's Date"
            name="settings.subject.showTodaysDate"
            tooltip="Whether today's date will be included in the report subject line"
          />
        </Row>
      </Col>
      <Row>
        <Col className="frm-in">
          <label>Report Options</label>
          <Row alignItems="center" flex="1">
            <FormikCheckbox
              label="View On Web Only"
              name="settings.viewOnWebOnly"
              tooltip="Email will only contain a link to view the report on the website"
            />
            <FormikCheckbox
              label="Exclude Historical Content"
              name="settings.instance.excludeHistorical"
              tooltip="Exclude content already reported on in prior instances of this report"
            />
          </Row>
        </Col>
        <FormikSelect
          name="settings.instance.excludeReports"
          label="Exclude Related Report Content"
          tooltip="Excludes content already reported on in the selected reports"
          options={reportOptions}
          isMulti
          value={reportOptions.filter((ro) =>
            values.settings.instance.excludeReports.some((reportId) => reportId === ro.value),
          )}
          onChange={(newValue) => {
            if (Array.isArray(newValue))
              setFieldValue(
                'settings.instance.excludeReports',
                newValue.map((v: OptionItem) => v.value),
              );
          }}
        />
      </Row>
      <Col className="frm-in">
        <label>Headline Options</label>
        <Row>
          <FormikCheckbox label="Show Source" name="settings.headline.showSource" />
          <FormikCheckbox label="Show Common Call" name="settings.headline.showShortName" />
          <FormikCheckbox label="Show Published On" name="settings.headline.showPublishedOn" />
          <FormikCheckbox label="Show Sentiment" name="settings.headline.showSentiment" />
        </Row>
      </Col>
      <Col className="frm-in">
        <label>Content Options</label>
        <Row>
          <FormikCheckbox label="Include Full Story" name="settings.content.includeStory" />
          {/* <FormikCheckbox label="Show Images" name="settings.content.showImage" />
          <FormikCheckbox label="Use Thumbnails" name="settings.content.useThumbnail" />
          <FormikCheckbox label="Highlight Keywords" name="settings.content.highlightKeywords" /> */}
        </Row>
      </Col>
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
                <Col className="st-1">{row.sortOrder + 1}</Col>
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
