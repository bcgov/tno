import { ToggleButton } from 'components/buttons';
import { highlight, languages } from 'prismjs';
import React from 'react';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';
import Editor from 'react-simple-code-editor';
import { Col, OptionItem, Row, Select, Text } from 'tno-core';

import { useChartTemplateContext } from '../ChartTemplateContext';
import * as styled from './styled';

export const ChartTemplatePreviewSources = () => {
  const { chartRequestForm, setChartRequestForm, filter, setFilter } = useChartTemplateContext();

  const [indexOptions] = React.useState([
    new OptionItem('Published', 'content'),
    new OptionItem('Unpublished', 'unpublished_content'),
  ]);
  const [show, setShow] = React.useState(false);

  return (
    <styled.ChartTemplatePreviewSources>
      <Row justifyContent="space-between">
        <h3>Data Sources</h3>
        <span>
          Manually create a Elasticsearch query, or reference an existing report to dynamically
          fetch content to preview this chart.
        </span>
        <ToggleButton
          on={<FaMinus />}
          off={<FaAngleDown />}
          onClick={() => setShow(!show)}
          value={show}
        />
      </Row>
      {show && (
        <Row gap="1rem">
          <Col flex="2">
            <label>Elasticsearch Filter</label>
            <Row nowrap>
              <Select
                label="Index"
                name="index"
                options={indexOptions}
                value={indexOptions.find((o) => o.value === chartRequestForm.index) ?? ''}
                clearValue={''}
                onChange={(newValue) => {
                  const option = newValue as OptionItem;
                  setChartRequestForm({ ...chartRequestForm, index: option.value?.toString() });
                }}
              />
              <Col flex="1">
                <label htmlFor="txa-template">Query</label>
                <Col className="editor">
                  <Editor
                    id="txa-filter"
                    value={filter}
                    onValueChange={(code) => setFilter(code)}
                    highlight={(code) => {
                      return highlight(code, languages.json, 'razor');
                    }}
                  />
                </Col>
              </Col>
            </Row>
          </Col>
          <Col flex="1">
            <label>Linked Report</label>
            <Row>
              <Text
                name="linkedReportId"
                label="Report ID"
                value={chartRequestForm.linkedReportId ?? ''}
                type="number"
                onChange={(e) => {
                  setChartRequestForm({
                    ...chartRequestForm,
                    linkedReportId: e.target.value ? +e.target.value : undefined,
                  });
                }}
              />
            </Row>
          </Col>
        </Row>
      )}
    </styled.ChartTemplatePreviewSources>
  );
};
