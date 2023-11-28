import {
  FaClipboard,
  FaCog,
  FaCogs,
  FaComment,
  FaEnvelope,
  FaFire,
  FaFolder,
  FaHome,
  FaMoon,
  FaNewspaper,
  FaSearch,
  FaSlidersH,
  FaUsers,
  FaUserTie,
} from 'react-icons/fa';

export interface ISideBarMenuItems {
  [key: string]: {
    label: string;
    path: string;
    icon: JSX.Element;
    secondaryIcon?: JSX.Element;
    secondaryIconRoute?: string;
  };
}

/** The below manages the items that will appear in the left navigation bar in sequential order. */
export const SidebarMenuItems: ISideBarMenuItems = {
  home: {
    label: 'Home',
    path: 'landing/home',
    icon: <FaHome />,
  },
  topStories: {
    label: 'Top Stories',
    path: 'landing/topstories',
    icon: <FaFire />,
  },
  todaysCommentary: {
    label: "Today's Commentary",
    path: 'landing/todayscommentary',
    icon: <FaComment />,
  },
  todaysFrontPages: {
    label: "Today's Front Pages",
    path: 'landing/todaysfrontpages',
    icon: <FaNewspaper />,
  },
  pressGallery: {
    label: 'Press Gallery',
    path: 'landing/pressgallery',
    icon: <FaUsers />,
  },
  eveningOverview: {
    label: 'Evening Overview',
    path: 'landing/eveningoverview',
    icon: <FaMoon />,
  },
  filterMedia: {
    label: 'Filter by media type',
    path: 'landing/filtermedia',
    icon: <FaSlidersH />,
  },
  myProducts: {
    label: 'MMI Products',
    path: 'products',
    icon: <FaEnvelope />,
  },
  myMinister: {
    label: 'My Minister',
    path: 'landing/myminister',
    icon: <FaUserTie />,
    secondaryIcon: <FaCog />,
    secondaryIconRoute: '/landing/settings',
  },
  mySearches: {
    label: 'My Saved Searches',
    path: 'landing/mysearches',
    icon: <FaSearch />,
  },
  folders: {
    label: 'My Folders',
    path: 'landing/folders',
    icon: <FaFolder />,
  },
  myReports: {
    label: 'My Reports',
    path: 'reports',
    icon: <FaClipboard />,
  },
  settings: {
    label: 'Settings',
    path: 'landing/settings',
    icon: <FaCogs />,
  },
};

export const sidebarMenuItemsArray = Object.keys(SidebarMenuItems).map(
  (key) => SidebarMenuItems[key],
);
