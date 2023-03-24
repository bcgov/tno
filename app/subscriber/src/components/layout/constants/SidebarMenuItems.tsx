import {
  FaClipboard,
  FaComment,
  FaFire,
  FaHeart,
  FaHome,
  FaSlidersH,
  FaUsers,
  FaUserTie,
} from 'react-icons/fa';

export interface ISideBarMenuItems {
  [key: string]: { label: string; path: string; icon: JSX.Element };
}

/** The below manages the items that will appear in the left navigation bar in sequential order. */
export const SidebarMenuItems: ISideBarMenuItems = {
  home: {
    label: 'Home',
    path: 'home',
    icon: <FaHome />,
  },
  topStories: {
    label: 'Top Stories',
    path: 'topstories',
    icon: <FaFire />,
  },
  myMinister: {
    label: 'My Minister',
    path: 'myminister',
    icon: <FaUserTie />,
  },
  todaysCommentary: {
    label: "Today's Commentary",
    path: 'todayscommentary',
    icon: <FaComment />,
  },
  pressGallery: {
    label: 'Press Gallery',
    path: 'pressgallery',
    icon: <FaUsers />,
  },
  filterMedia: {
    label: 'Filter media type',
    path: 'filtermedia',
    icon: <FaSlidersH />,
  },
  myCollections: {
    label: 'My Collections',
    path: 'mycollections',
    icon: <FaHeart />,
  },
  myReports: {
    label: 'My Reports',
    path: 'myreports',
    icon: <FaClipboard />,
  },
};

export const sidebarMenuItemsArray = Object.keys(SidebarMenuItems).map(
  (key) => SidebarMenuItems[key],
);
