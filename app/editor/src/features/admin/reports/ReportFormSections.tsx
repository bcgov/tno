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
        .catch(() => {});
    // Only run on initialize.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleAddSection = () => {
    setFieldValue(
      'sections',
      [...values.sections, defaultReportSection(values.id)].map((section, index) => {
        return {
          ...section,
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
      sections.map((s, i) => ({
        ...s,
        sortOrder: i,
      })),
    );
  };

  return (
    <styled.ReportSections>
      <h2>{values.name}</h2>
      <p>A report must contain one or more section to display content or charts.</p>
      <Row gap="1rem">
        <Col className="frm-in options" flex="1">
          <label>Subject Line Options</label>
          <Col>
            <p>Customize the email subject line.</p>
            <Row alignItems="center">
              <Col flex="1">
                <FormikText label="Text" name="settings.subject.text" required />
              </Col>
              <FormikCheckbox
                label="Show Today's Date"
                name="settings.subject.showTodaysDate"
                tooltip="Whether today's date will be included in the report subject line"
              />
            </Row>
          </Col>
        </Col>
        <Col className="frm-in options" flex="1">
          <label>Report Options</label>
          <Col>
            <p>
              Control the output of the report. Redirect users to view the report on the website
              instead of in their email.
            </p>
            <FormikCheckbox
              label="View On Web Only"
              name="settings.viewOnWebOnly"
              tooltip="Email will only contain a link to view the report on the website"
            />
            <FormikCheckbox
              label="Show Link to Story"
              name="settings.content.showLinkToStory"
              tooltip="Include a link to view the story online"
            />
            <FormikCheckbox
              label="Highlight Keywords"
              name="settings.content.highlightKeywords"
              tooltip="Highlight the search keywords found in the report content"
            />
          </Col>
        </Col>
      </Row>
      <Row gap="1rem">
        <Col className="frm-in options" flex="1">
          <label>Headline Options</label>
          <Col>
            <p>Control what is displayed for each headline in the report.</p>
            <Row>
              <FormikCheckbox label="Show Source" name="settings.headline.showSource" />
              <FormikCheckbox label="Show Common Call" name="settings.headline.showShortName" />
              <FormikCheckbox label="Show Published On" name="settings.headline.showPublishedOn" />
              <FormikCheckbox label="Show Sentiment" name="settings.headline.showSentiment" />
            </Row>
          </Col>
        </Col>
        <Col className="frm-in options" flex="2">
          <label>Content Options</label>
          <Col>
            <p>
              Control what content is included in this report by removing content from prior
              instances or related reports.
            </p>
            <Row gap="1rem">
              <Col gap="1rem">
                <FormikCheckbox
                  label="Exclude stories that have been sent out in previous report"
                  name="settings.content.excludeHistorical"
                  tooltip="Exclude content already reported on in prior instances of this report"
                />
                <FormikSelect
                  label="Excludes content already reported on in the selected reports"
                  name="settings.content.excludeReports"
                  tooltip="Excludes content already reported on in the selected reports"
                  options={reportOptions}
                  isMulti
                  value={reportOptions.filter((ro) =>
                    values.settings.content.excludeReports?.some(
                      (reportId) => reportId === ro.value,
                    ),
                  )}
                  onChange={(newValue) => {
                    if (Array.isArray(newValue))
                      setFieldValue(
                        'settings.content.excludeReports',
                        newValue.map((v: OptionItem) => v.value),
                      );
                  }}
                />
              </Col>
              <Col gap="1rem">
                <FormikCheckbox
                  label="Include only new content posted after previous report"
                  name="settings.content.onlyNewContent"
                  tooltip="Adds a date filter to only include content posted since the last time this report ran"
                />
                <FormikCheckbox
                  label="Clear all folders after report runs"
                  name="settings.content.clearFolders"
                  tooltip="Clears all content from all folders in this report after this report is run"
                />
              </Col>
            </Row>
          </Col>
        </Col>
      </Row>
      <Col className="frm-in options">
        <label>Section Options</label>
        <Col>
          <p>Control what sections are in the report, and how each section is presented.</p>
          <Row>
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
          <Col>
            <Col className="section-table">
              <Row className="row-header">
                <Col className="st-1"></Col>
                <Col className="st-2">Heading</Col>
                <Col className="st-3">Summary</Col>
                <Col className="st-4">Content</Col>
                <Col className="st-5"></Col>
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
                    <Col className="st-2">
                      {row.settings.label.length > 0 ? row.settings.label : '[empty]'}
                    </Col>
                    <Col className="st-3">{row.description}</Col>
                    <Col className="st-4">
                      {row.settings.sectionType === 'Content'
                        ? row.filter?.name
                        : row.settings.sectionType === 'TableOfContents'
                        ? 'Table of Contents'
                        : 'Summary'}
                    </Col>
                    <Col className="st-5">
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
            </Col>
          </Col>
        </Col>
      </Col>
    </styled.ReportSections>
  );
};
