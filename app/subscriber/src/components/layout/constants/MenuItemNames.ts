// enum that controls the items that will appear in the navigation menu, please note that if you are adding a new item
// you will need to add a custom icon for it in SelectableMenuItems.tsx otherwise it will use the default icon
export enum MenuItemNames {
  /** The name of the menu item for the home page. */
  Home = 'Home',
  TopStories = 'Top Stories',
  MyMinister = 'My Minister',
  TodaysCommentary = "Today's Commentary",
  PressGallery = 'Press Gallery',
  FilterMedia = 'Filter media type',
  MyCollections = 'My Collections',
  MyReports = 'My Reports',
}
