import { Action } from 'components/action';
import { MenuButton } from 'components/button';
import { FaLock } from 'react-icons/fa6';
import { FaCaretRight, FaRightToBracket } from 'react-icons/fa6';
import { Row, Show } from 'tno-core';

import { ReportKindIcon } from '../components';
import { getLastSent } from '../utils';
import {
  ReportContentMenuOption,
  ReportMainMenuOption,
  ReportSendMenuOption,
  ReportSettingsMenuOption,
} from './constants';
import { useReportEditContext } from './ReportEditContext';
import * as styled from './styled';

export interface IReportEditMenuProps {
  onChange?: (path: string) => void;
}

export const ReportEditMenu = ({ onChange }: IReportEditMenuProps) => {
  const { active, values, errors, isValid } = useReportEditContext();

  const instance = values.instances.length ? values.instances[0] : undefined;
  const lastSent = getLastSent(values);

  return (
    <styled.ReportEditMenu className="report-menu">
      <div className="report-headline">
        <div>
          <Action
            icon={<FaRightToBracket className="icon-exit" />}
            onClick={() => onChange?.(`/reports`)}
          />
        </div>
        <div>
          <div className="report-name">
            {values.name ? values.name : 'New Report'} <ReportKindIcon report={values} />
          </div>
        </div>
        <Row nowrap gap="0.5rem">
          <label>Last Sent:</label>
          <span>{lastSent ? lastSent : 'Never'}</span>
        </Row>
      </div>

      <div className="report-main-menu">
        <div>
          <MenuButton
            label="Settings"
            active={!active || active?.startsWith(ReportMainMenuOption.Settings)}
            onClick={() => {
              if (!values.id) onChange?.(`/reports/${values.id}/${ReportMainMenuOption.Settings}`);
              else onChange?.(`/reports/${values.id}/${ReportSettingsMenuOption.Sections}`);
            }}
            className={
              !isValid && !active?.startsWith(ReportMainMenuOption.Settings) ? 'error' : ''
            }
          ></MenuButton>
        </div>
        <div>
          <MenuButton
            label={
              <Row gap="0.5rem" alignItems="center">
                Content
                <Show visible={!!instance?.sentOn}>
                  <FaLock />
                </Show>
              </Row>
            }
            active={active?.startsWith(ReportMainMenuOption.Content)}
            disabled={!values.id}
            onClick={() => onChange?.(`/reports/${values.id}/${ReportMainMenuOption.Content}`)}
          ></MenuButton>
        </div>
        <div>
          <MenuButton
            label="Report Preview"
            active={active?.startsWith(ReportMainMenuOption.View)}
            disabled={!values.id}
            onClick={() => onChange?.(`/reports/${values.id}/${ReportMainMenuOption.View}`)}
          ></MenuButton>
        </div>
        <div>
          <MenuButton
            label="Send"
            active={active === ReportMainMenuOption.Send}
            onClick={() => onChange?.(`/reports/${values.id}/${ReportMainMenuOption.Send}`)}
          />
        </div>
      </div>
      <div className="report-secondary-menu">
        <Show visible={active?.startsWith(ReportMainMenuOption.Settings)}>
          <div>
            <MenuButton
              label="Info"
              active={active === ReportSettingsMenuOption.Info}
              onClick={() => onChange?.(`/reports/${values.id}/${ReportSettingsMenuOption.Info}`)}
              className={errors.name || errors.settings?.subject?.text ? 'error' : ''}
            >
              <FaCaretRight className="caret" />
            </MenuButton>
          </div>
          <div>
            <MenuButton
              label="Template Design"
              active={active === ReportSettingsMenuOption.Sections}
              onClick={() =>
                onChange?.(`/reports/${values.id}/${ReportSettingsMenuOption.Sections}`)
              }
              className={errors.sections ? 'error' : ''}
            >
              <FaCaretRight className="caret" />
            </MenuButton>
          </div>
          <div>
            <MenuButton
              label="Data Sources"
              active={active === ReportSettingsMenuOption.DataSources}
              onClick={() =>
                onChange?.(`/reports/${values.id}/${ReportSettingsMenuOption.DataSources}`)
              }
              className={errors.sections ? 'error' : ''}
            >
              <FaCaretRight className="caret" />
            </MenuButton>
          </div>
          <div>
            <MenuButton
              label="Report Preferences"
              active={active === ReportSettingsMenuOption.Preferences}
              onClick={() =>
                onChange?.(`/reports/${values.id}/${ReportSettingsMenuOption.Preferences}`)
              }
            >
              <FaCaretRight className="caret" />
            </MenuButton>
          </div>
          <div>
            <MenuButton
              label="Sending"
              active={active === ReportSettingsMenuOption.Send}
              onClick={() => onChange?.(`/reports/${values.id}/${ReportSettingsMenuOption.Send}`)}
              className={errors.events ? 'error' : ''}
            />
          </div>
        </Show>
        {/* Content secondary menu */}
        <Show visible={active?.startsWith(ReportMainMenuOption.Content)}>
          <div>
            <MenuButton
              label={
                <Row gap="0.5rem" alignItems="center">
                  Curate Stories
                  <Show visible={!!instance?.sentOn}>
                    <FaLock />
                  </Show>
                </Row>
              }
              active={active === ReportMainMenuOption.Content}
              onClick={() => onChange?.(`/reports/${values.id}/${ReportMainMenuOption.Content}`)}
            >
              <FaCaretRight className="caret" />
            </MenuButton>
          </div>
          <div>
            <MenuButton
              label={
                <Row gap="0.5rem" alignItems="center">
                  Quick Sort
                  <Show visible={!!instance?.sentOn}>
                    <FaLock />
                  </Show>
                </Row>
              }
              active={active === ReportContentMenuOption.Sort}
              onClick={() => onChange?.(`/reports/${values.id}/${ReportContentMenuOption.Sort}`)}
            >
              <FaCaretRight className="caret" />
            </MenuButton>
          </div>
          <div>
            <MenuButton
              label={
                <Row gap="0.5rem" alignItems="center">
                  Executive Summary
                  <Show visible={!!instance?.sentOn}>
                    <FaLock />
                  </Show>
                </Row>
              }
              active={active === ReportContentMenuOption.Summary}
              onClick={() => onChange?.(`/reports/${values.id}/${ReportContentMenuOption.Summary}`)}
            />
          </div>
        </Show>
        {/* Preview secondary menu */}
        <Show visible={active?.startsWith(ReportMainMenuOption.View)}>
          <MenuButton
            label="View"
            active={active === ReportMainMenuOption.View}
            onClick={() => onChange?.(`/reports/${values.id}/${ReportMainMenuOption.View}`)}
          />
        </Show>
        {/* Send secondary menu */}
        <Show visible={active?.startsWith(ReportMainMenuOption.Send)}>
          <MenuButton
            label="Subscribers"
            active={active === ReportSendMenuOption.Send}
            onClick={() => onChange?.(`/reports/${values.id}/${ReportSendMenuOption.Send}`)}
          />
          <MenuButton
            label="History"
            active={active === ReportSendMenuOption.History}
            onClick={() => onChange?.(`/reports/${values.id}/${ReportSendMenuOption.History}`)}
          />
        </Show>
      </div>
    </styled.ReportEditMenu>
  );
};
