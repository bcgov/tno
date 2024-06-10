import { Modal } from 'components/modal';
import React from 'react';
import { FaEnvelope } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { useColleagues, useLookup } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Button,
  IContentModel,
  IUserColleagueModel,
  Row,
  Settings,
  Text,
  useModal,
} from 'tno-core';

import * as styled from './styled';

export interface IShareSubMenuProps {
  /**
   * The content that has been selected to add to a folder
   */
  content: IContentModel[];
}

/** Component that renders the button that gives users access to a sub menu that will allow them to add selected content to
 * an existing folder. Or create a new one.
 */
export const ShareMenu: React.FC<IShareSubMenuProps> = ({ content }) => {
  const [{ settings }] = useLookup();
  const [{ myColleagues, init }] = useProfileStore();
  const { toggle, isShowing } = useModal();
  const [{ getColleagues, share, shareEmail }] = useColleagues();
  const [user, setUser] = React.useState<IUserColleagueModel | null>(null);
  const [emailAddress, setEmailAddress] = React.useState<string>('');

  const handleSendColleague = async () => {
    try {
      const notificationId = settings.find((s) => s.name === Settings.DefaultAlert)?.value;
      if (notificationId) {
        content.forEach(async (c) => {
          if (user) {
            await share(c.id, user.colleagueId, parseInt(notificationId));
          }
        });
        toast.success('Notification has been successfully requested');
      } else {
        toast.error(`${Settings.DefaultAlert} setting needs to be configured.`);
      }
    } catch {}
  };

  const handleSendEmail = async () => {
    try {
      const notificationId = settings.find((s) => s.name === Settings.DefaultAlert)?.value;
      if (notificationId) {
        content.forEach(async (c) => {
          if (emailAddress !== '') {
            const resp = await shareEmail(c.id, emailAddress, parseInt(notificationId));
            if (resp) {
              toast.success('Notification has been successfully requested');
            }
          }
        });
      } else {
        toast.error(`${Settings.DefaultAlert} setting needs to be configured.`);
      }
    } catch {}
  };

  React.useEffect(() => {
    if (!init.myColleagues) {
      getColleagues().catch(() => {});
    }
  }, [getColleagues, init.myColleagues]);

  const message =
    content.length > 0
      ? `Share ${content.length} selected content${content.length > 1 ? 's' : ''} with ${
          emailAddress !== ''
            ? emailAddress
            : user?.colleague?.preferredEmail !== ''
            ? user?.colleague?.preferredEmail
            : user?.colleague?.email
        } ?`
      : `Please select stories to share with your colleague.`;

  return (
    <styled.ShareMenu className="share-sub-menu">
      <Row justifyContent="end">
        <div className="action" data-tooltip-id="share">
          <FaEnvelope /> <span className="share-text">Share</span>
        </div>
      </Row>
      <Tooltip
        clickable
        variant="light"
        className="share-menu"
        place="bottom"
        openOnClick
        style={{ opacity: '1', boxShadow: '0 0 8px #464545', zIndex: '999' }}
        id="share"
      >
        <FaEnvelope /> SHARE WITH A COLLEAGUE:
        <ul>
          {myColleagues.map((o) => {
            return (
              <li
                key={
                  o.colleague?.preferredEmail !== ''
                    ? o.colleague?.preferredEmail
                    : o.colleague?.email
                }
                onClick={() => {
                  setEmailAddress('');
                  setUser(o);
                  toggle();
                }}
              >
                {o.colleague?.displayName !== ''
                  ? o.colleague?.displayName
                  : o.colleague?.preferredEmail !== ''
                  ? o.colleague?.preferredEmail
                  : o.colleague?.email}
              </li>
            );
          })}
        </ul>
        <Row className="add-row">
          <Text
            placeholder="Share with email..."
            name="email"
            onChange={(e) => setEmailAddress(e.target.value)}
          />
          <Button
            className="share-email"
            onClick={() => {
              setUser(null);
              toggle();
            }}
          >
            Share
          </Button>
        </Row>
      </Tooltip>
      <Modal
        headerText="Share Content"
        body={message}
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Share"
        enableConfirm={content.length > 0 && (user?.colleague !== undefined || emailAddress !== '')}
        onConfirm={() => {
          emailAddress !== '' ? handleSendEmail() : handleSendColleague();
          toggle();
        }}
      />
    </styled.ShareMenu>
  );
};
